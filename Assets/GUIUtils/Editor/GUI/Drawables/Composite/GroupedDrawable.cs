using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class GroupedDrawable : CompositeDrawableMember
    {
        private struct GroupInfo
        {
            public GroupedDrawable Group;
            public PropertyGroupAttribute Attribute;
        }

        protected readonly Dictionary<string, GroupedDrawable> _subGroupsByName = new Dictionary<string, GroupedDrawable>();
        protected readonly Dictionary<IOrderedDrawable, SizeInfo> _sizeInfoByDrawable = new Dictionary<IOrderedDrawable, SizeInfo>();

        protected readonly GroupedDrawable _parent;
        protected readonly SizeInfo _size;
        
        protected GroupedDrawable(GroupedDrawable parent, string groupID, float order)
            : base(groupID, order)
        {
            _parent = parent;
            Name = groupID;
            Order = order;
            _size = new SizeInfo();
        }

        public override void AddRange(ICollection<IOrderedDrawable> children)
        {
            var attributes = new List<PropertyGroupAttribute>();
            foreach (var child in children)
                attributes.AddRange(child.GetDrawableAttributes<PropertyGroupAttribute>());

            EnsureAllGroupsExist(attributes);
            
            base.AddRange(children);
        }

        public override void Add(IOrderedDrawable child)
        {
            var groupAttributes = child.GetDrawableAttributes<PropertyGroupAttribute>();
            if (groupAttributes.IsNullOrEmpty())
            {
                base.Add(child);
                return;
            }

            var finalPairs = EnsureAllGroupsExist(groupAttributes);

            // Add it to our final leaves
            foreach (var pair in finalPairs)
            {
                pair.Group.AddToGroup(child, pair.Attribute);
                EnsureGroupIsDrawn(pair.Group);
            }
        }

        private void EnsureGroupIsDrawn(GroupedDrawable subGroup)
        {
            foreach (var group in _subGroupsByName.Values)
            {
                if (!group.IsHostFor(subGroup)) continue;
                
                base.Add(group);
                if (!_sizeInfoByDrawable.ContainsKey(group))
                    _sizeInfoByDrawable.Add(group, group._size);

                
                group.EnsureGroupIsDrawn(subGroup);
            }
        }

        private List<GroupInfo> EnsureAllGroupsExist(ICollection<PropertyGroupAttribute> groupAttributes)
        {
            var finalGroups = new List<GroupInfo>();
            // keep track of attempts
            // when this exceeds list count, we've gone through the entire list without resolving an attribute and we can give up
            int attempts = 0;
            var attributesQueue = new Queue<PropertyGroupAttribute>(groupAttributes);
            while (attributesQueue.Any() && attributesQueue.Count > attempts)
            {
                // If we managed to get or create our group, reset attempts and remove our entry
                if (TryGetOrCreateGroup(attributesQueue.Peek(), out GroupedDrawable group))
                {
                    attempts = 0;
                    var attribute = attributesQueue.Dequeue();

                    bool isFinalGroup = true;
                    for (var i = finalGroups.Count - 1; i >= 0; i--)
                    {
                        if (group.IsHostFor(finalGroups[i].Group))
                        {
                            isFinalGroup = false;
                            break;
                        }

                        if (finalGroups[i].Group.IsHostFor(group))
                            finalGroups.RemoveAt(i);
                    }

                    if (isFinalGroup)
                    {
                        finalGroups.Add(new GroupInfo
                        {
                            Group = group,
                            Attribute = attribute
                        });
                    }
                }
                else // we increase our attempts and move our current item to the back of the list
                {
                    ++attempts;
                    var item = attributesQueue.Dequeue();
                    attributesQueue.Enqueue(item);
                }
            }

            return finalGroups;
        }

        private bool IsHostFor(GroupedDrawable subGroup)
        {
            if (this == subGroup) return true;

            foreach (var group in _subGroupsByName.Values)
            {
                if (group.IsHostFor(subGroup))
                    return true;
            }

            return false;
        }

        private void AddToGroup(IOrderedDrawable child, PropertyGroupAttribute groupAttribute)
        {
            if (groupAttribute != null)
            {
                RegisterDrawable(child, groupAttribute);
            }
            else
                throw new ArgumentNullException(nameof(groupAttribute));
        }

        protected virtual void RegisterDrawable(IOrderedDrawable child, PropertyGroupAttribute groupAttribute)
        {
            base.Add(child);
            ParseAttribute(child, groupAttribute);
            // TODO attr stuff
        }

        protected abstract void ParseAttribute(IOrderedDrawable child, PropertyGroupAttribute attr);
        
        protected abstract void ParseAttribute(PropertyGroupAttribute attr);

        protected void SetOrder(float order)
        {
            Order = order;
        }

        public bool TryGetOrCreateGroup(PropertyGroupAttribute groupAttribute, out GroupedDrawable group)
        {
            var parts = GroupingHelper.SplitIntoParts(groupAttribute.GroupID);
            return TryGetOrCreateGroup(parts.TakeSegment(0), groupAttribute, out group);
        }

        protected bool TryGetOrCreateGroup(ArraySegment<string> groupIdParts, PropertyGroupAttribute groupAttribute, out GroupedDrawable finalGroup)
        {
            // if we've reached the final leaf, return ourselves and that we managed to find it
            if (groupIdParts.Count == 0)
            {
                ParseAttribute(groupAttribute);
                finalGroup = this;
                return true;
            }
            
            // if not, we check the next group
            var next = groupIdParts.First();
            GroupedDrawable nextGroup = _subGroupsByName.GetOrDefault(next);
            // If we can't find it...
            if (nextGroup == null)
            {
                // Create the group if we've reached the final leaf
                if (groupIdParts.Count == 1)
                {
                    nextGroup = GroupingHelper.CreateFrom(groupAttribute, this);
                    // We don't add them yet, add the group when it is used to preserve property order
                    // base.Add(nextGroup);
                    _subGroupsByName.Add(next, nextGroup);
                }
                else // We cannot create subgroups...
                {
                    finalGroup = null;
                    return false;
                }
            }
            
            // Else pass it to the next group
            var nextParts = groupIdParts.TakeSegment(1);
            return nextGroup.TryGetOrCreateGroup(nextParts, groupAttribute, out finalGroup);
        }

        public void EnsureSizeFits(SizeInfo size)
        {
            if (_size.MaxSize > 0)
            {
                float width = _size.MaxSize - CustomGUIUtility.Padding;
                if (size.MaxSize <= float.Epsilon || size.MaxSize > width)
                    size.MaxSize = width;
            }
            
            if (_size.PreferredSize > 0)
            {
                float width = _size.PreferredSize - CustomGUIUtility.Padding;
                if (size.PreferredSize > width)
                    size.PreferredSize = width;

                if (size.MaxSize <= float.Epsilon || size.MaxSize > width)
                    size.MaxSize = width;
            }

            _parent?.EnsureSizeFits(size);
        }
        
        protected GUILayoutOption[] GetLayoutOptions(SizeInfo info)
        {
            var list = new List<GUILayoutOption>();
            if (info.MinSize > 0.0f)
                list.Add(GUILayout.MinWidth(info.MinSize));
            if (info.PreferredSize > 0.0f)
                list.Add(GUILayout.Width(info.PreferredSize));
            if (info.MaxSize > 0.0f)
                list.Add(GUILayout.MaxWidth(info.MaxSize));
            
            return list.ToArray();
        }
    }
}