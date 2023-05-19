using System;
using System.Collections;
using System.Collections.Generic;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class FilteredCollection
    {
        public readonly IEnumerable Options;
        public readonly int AmountOfOptions;
        public readonly List<object> FilteredValues = new List<object>();
        
        public static FilteredCollection Create<T>(
            ICollection<T> options,
            Func<T, string> textSelector,
            Func<T, string> subtextSelector)
            => new TypedFilteredCollection<T>(options, textSelector, subtextSelector);

        // Note: needs to be IEnumerable and not ICollection, because for some reason, ICollection<T> does not inherit from ICollection...
        protected FilteredCollection(IEnumerable options, int optionsCount)
        {
            Options = options;
            AmountOfOptions = optionsCount + 1; // + 1 for Null
        }

        public void UpdateSearch(string filter)
        {
            FilteredValues.Clear();
            FilteredValues.Add(null);
            foreach (var value in Options)
            {
                if (MatchesFilter(value, filter))
                    FilteredValues.Add(value);
            }
        }

        protected abstract bool MatchesFilter(object o, string filter);
        public abstract string GetTextFor(object o);
        public abstract string GetSubTextFor(object o);
    }
}