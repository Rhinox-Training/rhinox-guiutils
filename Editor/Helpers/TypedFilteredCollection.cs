using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;

namespace Rhinox.GUIUtils.Editor
{
    public class TypedFilteredCollection<T> : FilteredCollection
    {
        protected Func<T, string> _textSelector;
        protected Func<T, string> _subTextSelector;
        
        public TypedFilteredCollection(ICollection<T> options, Func<T, string> textSelector, Func<T, string> subTextSelector)
            : base(options, options.Count)
        {
            _textSelector = textSelector;
            _subTextSelector = subTextSelector;
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
    }
}