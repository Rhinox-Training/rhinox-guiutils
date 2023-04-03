using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public static class GroupingHelper
    {
        private const string GroupingString = "/";
        
        public static GroupedDrawable CreateFrom(PropertyGroupAttribute groupingAttr, GroupedDrawable parent = null)
        {
            if (groupingAttr is TitleGroupAttribute)
                return new TitleGroupDrawable(parent, groupingAttr.GroupID, groupingAttr.Order);
            
            if (groupingAttr is HorizontalGroupAttribute || groupingAttr is ButtonGroupAttribute)
                return new HorizontalGroupDrawable(parent, groupingAttr.GroupID, groupingAttr.Order);

            return new VerticalGroupDrawable(parent, groupingAttr.GroupID, groupingAttr.Order);
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