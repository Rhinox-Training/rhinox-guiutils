using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public static class DrawableGroupingHelper
    {
        private const string GroupingString = "/";
        
        public static void Process(ref List<IOrderedDrawable> drawables)
        {
            var drawablesByGroup = CreateLookupByGroupAttribute(drawables);
            if (drawablesByGroup.IsNullOrEmpty())
                return;

            var remainingGroupings = drawablesByGroup.Keys.ToList();
            remainingGroupings.SortByDescending(x => x.GroupID.Length);

            // Create composites (unconnected)
            var idLookup = new Dictionary<string, CompositeDrawableMember>();
            var groupingAttrLookup = new Dictionary<CompositeDrawableMember, PropertyGroupAttribute>();
            while (remainingGroupings.Count > 0)
            {
                var currentGroupAttr = remainingGroupings.First();
                remainingGroupings.RemoveAt(0);

                var compositeMember = CompositeDrawableMember.CreateFrom(currentGroupAttr);
                var entries = drawablesByGroup[currentGroupAttr];

                foreach (var entry in entries)
                {
                    if (entry == null)
                        continue;
                    compositeMember.Add(entry.Drawable, entry.Attribute);
                }

                // Clean entries from higher located composite groups
                foreach (var key in drawablesByGroup.Keys)
                {
                    if (!IsParentOf(currentGroupAttr, key))
                        continue;
                    var curDrawables = drawablesByGroup[key];

                    curDrawables.RemoveAll(x => entries.Any(e => e.Drawable == x.Drawable));
                }

                // Store in lookup
                idLookup.Add(currentGroupAttr.GroupID, compositeMember);
                groupingAttrLookup.Add(compositeMember, currentGroupAttr);
            }

            var finalList = new List<IOrderedDrawable>();
            // Create tree structure
            foreach (var drawable in drawables)
            {
                var groupingAttributes = drawable.GetDrawableAttributes<PropertyGroupAttribute>();
                if (groupingAttributes.IsNullOrEmpty())
                {
                    finalList.Add(drawable);
                    continue;
                }

                foreach (var groupingAttribute in groupingAttributes.OrderByDescending(x => x.GroupID.Length))
                {
                    string groupId = groupingAttribute.GroupID;

                    var parts = groupId.Split(new[] {GroupingString}, StringSplitOptions.RemoveEmptyEntries);
                    CompositeDrawableMember curParent = null;
                    for (int i = 0; i < parts.Length; ++i)
                    {
                        string idAtCurrentDepth = string.Join(GroupingString, parts.Take(i + 1));
                        if (idLookup.ContainsKey(idAtCurrentDepth))
                        {
                            if (curParent != null)
                            {
                                if (!curParent.Children.Contains(idLookup[idAtCurrentDepth]))
                                    curParent.Add(idLookup[idAtCurrentDepth], groupingAttrLookup.GetOrDefault(idLookup[idAtCurrentDepth]));
                            }

                            curParent = idLookup[idAtCurrentDepth];
                        }
                        else
                        {
                            var next = new CompositeDrawableMember(idAtCurrentDepth, groupingAttribute.Order);
                            if (curParent == null)
                                finalList.Add(next);
                            else
                                curParent.Add(next);
                            curParent = next;
                            idLookup.Add(idAtCurrentDepth, next);
                        }
                    }
                }
            }

            // Add root most groupings to finalList
            foreach (var entry in idLookup.Keys.ToArray())
            {
                var group = idLookup[entry];
                bool isTopLevel = true;
                foreach (var otherGroup in idLookup.Values)
                {
                    if (otherGroup == group)
                        continue;

                    if (otherGroup.Children.Contains(group))
                    {
                        isTopLevel = false;
                        break;
                    }
                }

                if (isTopLevel)
                    finalList.Add(group);
            }

            // Return list
            drawables = finalList;
        }

        private static bool IsParentOf(PropertyGroupAttribute entry, PropertyGroupAttribute potentialParent)
        {
            if (entry.GroupID == null)
                return false;
            return entry.GroupID.StartsWith(potentialParent.GroupID);
        }

        private class DrawableGroupEntry
        {
            public IOrderedDrawable Drawable;
            public PropertyGroupAttribute Attribute;
        }
        
        private static Dictionary<PropertyGroupAttribute, List<DrawableGroupEntry>> CreateLookupByGroupAttribute(
            List<IOrderedDrawable> drawables)
        {
            var grouping = new Dictionary<PropertyGroupAttribute, List<DrawableGroupEntry>>();
            foreach (var drawable in drawables)
            {
                if (drawable == null)
                    continue;

                var groupingAttributes = drawable.GetDrawableAttributes<PropertyGroupAttribute>();
                if (groupingAttributes.IsNullOrEmpty())
                {
                    continue;
                }

                foreach (var attr in groupingAttributes)
                {
                    var key = FindKey(grouping, attr);
                    if (key == null)
                    {
                        grouping.Add(attr, new List<DrawableGroupEntry>());
                        key = attr;
                    }

                    var list = grouping[key];

                    var entry = new DrawableGroupEntry()
                    {
                        Drawable = drawable,
                        Attribute = attr
                    };
                    
                    list.AddUnique(entry);
                    grouping[key] = list;
                }
            }

            return grouping;
        }

        private static PropertyGroupAttribute FindKey(
            Dictionary<PropertyGroupAttribute, List<DrawableGroupEntry>> grouping, PropertyGroupAttribute attr)
        {
            foreach (var group in grouping)
            {
                if (group.Key.GetType() != attr.GetType())
                    continue;

                if (group.Key.GroupID != null && group.Key.GroupID.Equals(attr.GroupID))
                {
                    return group.Key;
                }
            }

            return null;
        }
    }
}