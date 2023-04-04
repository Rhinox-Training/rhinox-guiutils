using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class GroupingHelper
    {
        private const string GroupingString = "/";
        
        public static GroupedDrawable CreateFrom(PropertyGroupAttribute groupingAttr, GroupedDrawable parent = null)
        {
            switch (groupingAttr)
            {
                case ButtonGroupAttribute buttonGroupAttribute:
                    return new ButtonGroupDrawable(parent, groupingAttr.GroupID, groupingAttr.Order);
                case HorizontalGroupAttribute horizontalGroupAttribute:
                    return new HorizontalGroupDrawable(parent, groupingAttr.GroupID, groupingAttr.Order);
                case TitleGroupAttribute titleGroupAttribute:
                    return new TitleGroupDrawable(parent, groupingAttr.GroupID, groupingAttr.Order);
                case VerticalGroupAttribute verticalGroupAttribute:
                    return new VerticalGroupDrawable(parent, groupingAttr.GroupID, groupingAttr.Order);
                default:
                    Debug.LogError($"Unimplemented Group type: {groupingAttr.GetType().Name}");
                    break;
            }

            return null;
        }

        public static string[] SplitIntoParts(string groupId)
        {
            return groupId.Split(new[] {GroupingString}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static GroupedDrawable Process(List<IOrderedDrawable> drawables)
        {
            var group = new VerticalGroupDrawable();
            group.AddRange(drawables);
            return group;
        }
    }
}