using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rhinox.GUIUtils.Editor
{
    public class CompositeDrawableMember : IOrderedDrawable
    {
        public string Name { get; }
        public float Order { get; set; }
        public string Title { get; private set; }
        public GUIStyle TitleStyle { get; private set; }
        public object Host { get; private set; }

        private readonly List<IOrderedDrawable> _drawableMemberChildren;
        private readonly Dictionary<IOrderedDrawable, PropertyGroupAttribute> _propertyGroupAttrByDrawable;
        private List<Attribute> _attributes;
        private bool _groupHorizontally;

        public IReadOnlyCollection<IOrderedDrawable> Children => _drawableMemberChildren != null ? 
            _drawableMemberChildren : (IReadOnlyCollection<IOrderedDrawable>)Array.Empty<IOrderedDrawable>();
        
        public virtual float ElementHeight
        {
            get
            {
                if (Children != null)
                {
                    if (_groupHorizontally)
                        return Children.Max(x => x.ElementHeight);
                    else
                    {
                        float height = 0.0f;
                        foreach (var child in Children)
                            height += child.ElementHeight;
                        return height;
                    }
                }

                return EditorGUIUtility.singleLineHeight;
            }
        }

        
        /// <summary>
        /// Enumerates as Depth First Search
        /// </summary>
        public IEnumerable<IOrderedDrawable> EnumerateTree(bool onlyLeaves = false)
        {
            foreach (var child in Children)
            {
                if (child is CompositeDrawableMember compositeChild)
                {
                    if (!onlyLeaves)
                        yield return child;
                    foreach (var grandChild in compositeChild.EnumerateTree())
                        yield return grandChild;
                }
                else
                {
                    yield return child;
                }
            }
        }

        public static CompositeDrawableMember CreateFrom(PropertyGroupAttribute groupingAttr)
        {
            var drawableMember = new CompositeDrawableMember(groupingAttr.GroupID, groupingAttr.Order);
            if (groupingAttr is HorizontalGroupAttribute || groupingAttr is ButtonGroupAttribute)
                drawableMember.GroupHorizontal();
            else
                drawableMember.GroupVertical();

            if (groupingAttr is TitleGroupAttribute titleAttr)
            {
                drawableMember.Title = titleAttr.GroupName;
                drawableMember.TitleStyle = titleAttr.BoldTitle ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title;
            }

            return drawableMember;
        }
        
        public CompositeDrawableMember(string name = null, float order = 0)
        {
            Name = name;
            Order = order;
            _drawableMemberChildren = new List<IOrderedDrawable>();
            _propertyGroupAttrByDrawable = new Dictionary<IOrderedDrawable, PropertyGroupAttribute>();
        }

        public void AddAttribute(Attribute attribute)
        {
            if (_attributes == null)
                _attributes = new List<Attribute>();
            _attributes.AddUnique(attribute);
        }

        public void Add(IOrderedDrawable child, PropertyGroupAttribute childAttribute = null)
        {
            if (child == null)
                return;

            if (_drawableMemberChildren.Contains(child))
                return;
            
            _drawableMemberChildren.Add(child);
            if (childAttribute != null)
                _propertyGroupAttrByDrawable.Add(child, childAttribute);
        }

        public void AddRange(ICollection<IOrderedDrawable> children)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child == null)
                        continue;
                    Add(child);
                }
            }
        }

        public ICollection<TAttribute> GetDrawableAttributes<TAttribute>() where TAttribute : Attribute
        {
            if (_attributes == null)
                return Array.Empty<TAttribute>();
            return _attributes.OfType<TAttribute>().ToList();
        }

        public virtual void Draw()
        {
            if (_drawableMemberChildren == null)
                return;

            StartGrouping();
            if (!string.IsNullOrWhiteSpace(Title))
                EditorGUILayout.LabelField(GUIContentHelper.TempContent(Title), TitleStyle);
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;

                float width = 0.0f;
                bool widthLimiting = _groupHorizontally && TryGetWidth(childDrawable, out width);
                if (widthLimiting)
                    GUILayout.BeginVertical(GUILayout.MaxWidth(width));
                childDrawable.Draw();
                if (widthLimiting)
                    GUILayout.EndVertical();
            }
            EndGrouping();
        }

        public virtual void Draw(Rect rect)
        {
            if (_drawableMemberChildren == null)
                return;
            
            if (!string.IsNullOrWhiteSpace(Title))
                EditorGUI.LabelField(rect, GUIContentHelper.TempContent(Title), TitleStyle);

            HandleRectGrouping(ref rect);
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                if (_groupHorizontally)
                {
                    if (TryGetWidth(childDrawable, out float width))
                    {
                        if (width <= 1.0f)
                            width = rect.width * width;
                        rect.width = width;
                    }
                }

                childDrawable.Draw(rect);

                if (_groupHorizontally)
                {
                    rect.x += rect.width;
                }
                else
                {
                    rect.y += rect.height;
                }
            }
        }

        private void HandleRectGrouping(ref Rect rect)
        {
            if (!_groupHorizontally)
                rect.height /= _drawableMemberChildren.Count;
            else
                rect.width /= _drawableMemberChildren.Count;
        }

        private bool TryGetWidth(IOrderedDrawable drawable, out float width)
        {
            if (_propertyGroupAttrByDrawable.ContainsKey(drawable))
            {
                var propAttr = _propertyGroupAttrByDrawable[drawable];
                if (propAttr != null)
                {
                    if (propAttr is HorizontalGroupAttribute horPropAttr && horPropAttr.Width > 0.0f)
                    {
                        width = horPropAttr.Width;
                        return true;
                    }
                }
            }

            width = 1.0f;
            return false;
        }

        private void StartGrouping()
        {
            if (!_groupHorizontally)
                GUILayout.BeginVertical();
            else
                GUILayout.BeginHorizontal();
        }

        private void EndGrouping()
        {
            if (!_groupHorizontally)
                GUILayout.EndVertical();
            else
                GUILayout.EndHorizontal();
        }

        public void Sort()
        {
            foreach (var drawable in Children)
            {
                if (drawable is CompositeDrawableMember compositeDrawableMember)
                    compositeDrawableMember.Sort();
            }

            _drawableMemberChildren.SortBy(x => x.Order);
        }
        
        private void GroupVertical() => _groupHorizontally = false;

        private void GroupHorizontal() => _groupHorizontally = true;
    }
}