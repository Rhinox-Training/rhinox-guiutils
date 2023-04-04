using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class CompositeDrawableMember : IOrderedDrawable
    {
        public string Name { get; protected set; }
        public float Order { get; set; }
        public object Host { get; }
        
        public bool IsVisible => true;
        public virtual GUIContent Label => GUIContent.none;

        protected readonly List<IOrderedDrawable> _drawableMemberChildren = new List<IOrderedDrawable>();
        private List<Attribute> _attributes;

        public IReadOnlyCollection<IOrderedDrawable> Children
            => _drawableMemberChildren ?? (IReadOnlyCollection<IOrderedDrawable>) Array.Empty<IOrderedDrawable>();
        
        public abstract float ElementHeight { get; }

        public event Action ChildrenChanged;

        protected CompositeDrawableMember(string name = null, float order = 0)
        {
            Name = name;
            Order = order;
        }
        
        public virtual IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>() where TAttribute : Attribute
        {
            if (_attributes == null)
                return Array.Empty<TAttribute>();
            return _attributes.OfType<TAttribute>();
        }

        public void AddAttribute(Attribute attribute)
        {
            if (_attributes == null)
                _attributes = new List<Attribute>();
            _attributes.AddUnique(attribute);
        }

        public virtual void Add(IOrderedDrawable child)
        {
            if (child == null)
                return;

            if (_drawableMemberChildren.Contains(child))
                return;
            
            _drawableMemberChildren.Add(child);

            OnChildrenChanged();
        }

        public virtual void AddRange(ICollection<IOrderedDrawable> children)
        {
            if (children == null) return;
            
            foreach (var child in children)
            {
                if (child == null)
                    continue;
                Add(child);
            }
        }
        
        public virtual void Draw(GUIContent label, params GUILayoutOption[] options)
            => Draw(label);

        public abstract void Draw(GUIContent label);
        
        public abstract void Draw(Rect rect, GUIContent label);

        public void Sort()
        {
            foreach (var drawable in Children)
            {
                if (drawable is CompositeDrawableMember compositeDrawableMember)
                    compositeDrawableMember.Sort();
            }

            _drawableMemberChildren.SortBy(x => x.Order);
            
            OnChildrenChanged();
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

        protected virtual void OnChildrenChanged()
        {
            ChildrenChanged?.Invoke();
        }
    }
}