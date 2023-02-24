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

            var remainingGroupings = drawablesByGroup.Keys.ToList();
            remainingGroupings.SortByDescending(x => x.GroupID.Length);

            // Create composites (unconnected)
            var idLookup = new Dictionary<string, CompositeDrawableMember>();
            while (remainingGroupings.Count > 0)
            {
                var currentGroupAttr = remainingGroupings.First();
                remainingGroupings.RemoveAt(0);

                var compositeMember = CompositeDrawableMember.CreateFrom(currentGroupAttr);
                var childDrawables = drawablesByGroup[currentGroupAttr];

                compositeMember.AddRange(childDrawables);

                // Clean entries from higher located composite groups
                foreach (var key in drawablesByGroup.Keys)
                {
                    var curDrawables = drawablesByGroup[key];
                    curDrawables.RemoveRange(childDrawables);
                }

                // Store in lookup
                idLookup.Add(currentGroupAttr.GroupID, compositeMember);
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
                                    curParent.Add(idLookup[idAtCurrentDepth]);
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
        
        private static Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>> CreateLookupByGroupAttribute(
            List<IOrderedDrawable> drawables)
        {
            var grouping = new Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>>();
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
                        grouping.Add(attr, new List<IOrderedDrawable>());
                        key = attr;
                    }

                    var list = grouping[key];
                    list.AddUnique(drawable);
                    grouping[key] = list;
                }
            }

            return grouping;
        }

        private static PropertyGroupAttribute FindKey(
            Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>> grouping, PropertyGroupAttribute attr)
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