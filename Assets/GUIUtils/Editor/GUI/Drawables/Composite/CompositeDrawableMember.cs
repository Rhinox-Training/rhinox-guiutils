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
        
        public bool IsVisible => true;
        public virtual GUIContent Label => GUIContent.none;

        private readonly List<IOrderedDrawable> _drawableMemberChildren;
        private readonly Dictionary<IOrderedDrawable, PropertyGroupAttribute> _propertyGroupAttrByDrawable;
        private List<Attribute> _attributes;
        private bool _groupHorizontally;
        private float[] _widths;
        private const float DEFAULT_PADDING = 2.0f;

        public IReadOnlyCollection<IOrderedDrawable> Children => _drawableMemberChildren != null ? 
            _drawableMemberChildren : (IReadOnlyCollection<IOrderedDrawable>)Array.Empty<IOrderedDrawable>();
        
        public virtual float ElementHeight
        {
            get
            {
                if (Children == null)
                    return EditorGUIUtility.singleLineHeight;
                
                if (_groupHorizontally)
                    return Children.Where(x => x.IsVisible).Max(x => x.ElementHeight);
                    
                float height = 0.0f;
                foreach (var child in Children)
                {
                    if (!child.IsVisible)
                        continue;
                    if (height > 0)
                        height += DEFAULT_PADDING;
                    height += child.ElementHeight;
                }
                return height;
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

        public virtual void Draw(GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            StartGrouping();
            
            if (!string.IsNullOrWhiteSpace(Title))
                EditorGUILayout.LabelField(GUIContentHelper.TempContent(Title), TitleStyle);
            
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;

                float width = 0.0f;
                bool widthLimiting = _groupHorizontally && TryGetWidth(childDrawable, out width);
                if (widthLimiting)
                    GUILayout.BeginVertical(GUILayout.MaxWidth(width));
                
                childDrawable.Draw(childDrawable.Label);
                
                if (widthLimiting)
                    GUILayout.EndVertical();
                
                GUILayout.Space(1); // padding
            }
            EndGrouping();
        }

        public virtual void Draw(Rect rect, GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            if (!string.IsNullOrWhiteSpace(Title))
            {
                var labelRect = rect.AlignTop(EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, GUIContentHelper.TempContent(Title), TitleStyle);
                rect.yMin += labelRect.height + DEFAULT_PADDING;
            }

            Rect childRect = rect;
            for (int i = 0; i < _drawableMemberChildren.Count; ++i)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;


                childRect.width = HandleRectWidthGrouping(rect, i);

                childRect.height = childDrawable.ElementHeight;
                childDrawable.Draw(childRect, childDrawable.Label);

                if (_groupHorizontally)
                    childRect.x += childRect.width + DEFAULT_PADDING;
                else
                    childRect.y += childRect.height + DEFAULT_PADDING;
            }
        }

        private float HandleRectWidthGrouping(Rect totalRect, int index)
        {
            if (!_groupHorizontally || !totalRect.IsValid())
                return totalRect.width;
            
            if (_widths == null || _widths.Length != _drawableMemberChildren.Count)
                RecreateWidthsArray(totalRect.width);

            return _widths[index];
        }

        private void RecreateWidthsArray(float totalWidth)
        {
            int zeroCount = 0;
            _widths = new float[_drawableMemberChildren.Count];
            for (int i = 0; i < _drawableMemberChildren.Count; ++i)
            {
                var child = _drawableMemberChildren[i];
                if (!TryGetWidth(child, out float width))
                {
                    ++zeroCount;
                    continue;
                }

                _widths[i] = width;
                if (width <= float.Epsilon)
                    ++zeroCount;
            }

            float usedWidth = 0.0f;
            for (int i = 0; i < _widths.Length; ++i)
            {
                var calcWidth = _widths[i];
                if (calcWidth <= float.Epsilon)
                    continue;

                if (calcWidth < 1.0f)
                    _widths[i] = calcWidth * totalWidth;
                usedWidth += _widths[i];
            }

            for (int i = 0; i < _widths.Length; ++i)
            {
                var calcWidth = _widths[i];
                if (calcWidth <= float.Epsilon)
                    _widths[i] = Mathf.Max(0, totalWidth - usedWidth) / zeroCount;
            }
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