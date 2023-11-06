using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TypedFilteredCollection<T> : FilteredCollection
    {
        protected Func<T, string> _textSelector;
        protected Func<T, string> _subTextSelector;
        protected Func<T, Texture> _iconSelector;
        protected List<T> _filteredValues = new List<T>();

        public override IList FilteredValues => _filteredValues;

        public TypedFilteredCollection(ICollection<T> options, Func<T, string> textSelector, Func<T, string> subTextSelector, Func<T, Texture> iconSelector = null)
            : base(options, options.Count)
        {
            _textSelector = textSelector;
            _subTextSelector = subTextSelector;
            _iconSelector = iconSelector;
        }

        protected override bool MatchesFilter(object value, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            var text = GetTextFor(value);
            if (!string.IsNullOrEmpty(text) && text.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                return true;

            var subText = GetSubTextFor(value);
            if (!string.IsNullOrEmpty(subText) && subText.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public override string GetTextFor(object o)
        {
            if (_textSelector == null) return o.ToString();
            return _textSelector.Invoke((T)o);
        }

        public override string GetSubTextFor(object o)
        {
            if (_subTextSelector == null) return null;
            return _subTextSelector.Invoke((T)o);
        }

        public override bool TryGetIconFor(object o, out Texture t)
        {
            if (_iconSelector == null)
            {
                t = null;
                return false;
            }
            
            t = _iconSelector.Invoke((T)o);
            return true;
        }
    }
}