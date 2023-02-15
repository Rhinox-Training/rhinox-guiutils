using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class CompositeDrawableMember : IOrderedDrawable
    {
        public int Order { get; set; }
        public PropertyGroupAttribute Grouping { get; private set; }
        
        private readonly ICollection<IOrderedDrawable> _drawableMemberChildren;

        public ICollection<IOrderedDrawable> Children => _drawableMemberChildren;

        public IOrderedDrawable FirstOrDefault(Func<IOrderedDrawable, bool> func = null)
        {
            if (func == null)
                return _drawableMemberChildren.FirstOrDefault();

            foreach (var child in _drawableMemberChildren)
            {
                if (child is CompositeDrawableMember compositeChild)
                {
                    return compositeChild.FirstOrDefault(func);
                }

                if (func.Invoke(child))
                    return child;
            }

            return null;
        }

        public CompositeDrawableMember(ICollection<IOrderedDrawable> subdrawables, int order = 0)
        {
            _drawableMemberChildren = subdrawables;
            Order = order;
        }

        public ICollection<TAttribute> GetMemberAttributes<TAttribute>() where TAttribute : Attribute
        {
            return Array.Empty<TAttribute>();
        }

        public void Draw()
        {
            if (_drawableMemberChildren == null)
                return;

            StartGrouping();
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                childDrawable.Draw();
            }
            EndGrouping();
        }

        public void Draw(Rect rect)
        {
            if (_drawableMemberChildren == null)
                return;

            StartGrouping();
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                childDrawable.Draw(rect);
            }
            EndGrouping();
        }

        public void GroupBy(PropertyGroupAttribute grouping)
        {
            Grouping = grouping;
        }

        private void StartGrouping()
        {
            if (Grouping == null)
                return;

            if (Grouping is HorizontalGroupAttribute horizontalAttr)
            {
                if (horizontalAttr.Width > 0.0f)
                    GUILayout.BeginHorizontal(GUILayout.Width(horizontalAttr.Width));
                else
                    GUILayout.BeginHorizontal();
            }
            else if (Grouping is VerticalGroupAttribute verticalAttr)
            {
                GUILayout.BeginVertical();
            }
            else
            {
                GUILayout.BeginHorizontal();
            }
        }

        private void EndGrouping()
        {
            if (Grouping == null)
                return;

            if (Grouping is HorizontalGroupAttribute horizontalAttr)
            {
                GUILayout.EndHorizontal();
            }
            else if (Grouping is VerticalGroupAttribute verticalAttr)
            {
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.EndHorizontal();
            }
        }
    }
}