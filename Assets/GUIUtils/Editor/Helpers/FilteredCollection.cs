using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class FilteredCollection
    {
        public readonly IEnumerable Options;
        public readonly int AmountOfOptions;
        public virtual IList FilteredValues { get; }
        
        // TODO: what is this doing here?
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
            FilteredValues.Add(GetDefaultEntry());
            foreach (var value in Options)
            {
                if (MatchesFilter(value, filter))
                    FilteredValues.Add(value);
            }
        }

        protected virtual object GetDefaultEntry()
        {
            return null;
        }

        protected abstract bool MatchesFilter(object o, string filter);
        public abstract string GetTextFor(object o);
        public abstract string GetSubTextFor(object o);
        
        public virtual bool TryGetIconFor(object o, out Texture icon)
        {
            icon = null;
            return false;
        }
    }
}