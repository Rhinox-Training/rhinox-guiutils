using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;

public class SizeInfo
{
    public float MinSize;
    public float PreferredSize;
    public float MaxSize;

    private static SizeInfo _empty;
    public static SizeInfo Empty => _empty ?? (_empty = new SizeInfo());
}

public class SizeManager : List<SizeInfo>
{
    private float _resolvedSize;
    private float _resolvedPadding;
    private float[] _resolvedArray;

    public void Set(ICollection<SizeInfo> infos)
    {
        Clear();
        AddRange(infos);
        ClearCache();
    }


    public float[] Resolve(float size, float padding)
    {
        size -= padding * Count - 1;

        if (_resolvedArray != null && _resolvedArray.Length == Count &&
            size.LossyEquals(_resolvedSize))
            return _resolvedArray;

        InternalResolve(size);
        return _resolvedArray;
    }

    private void InternalResolve(float totalSize)
    {
        _resolvedArray = new float[Count];
        _resolvedSize = totalSize;

        if (totalSize == 0) return;
        
        var flexibles = new List<int>(Enumerable.Range(0, Count));
        // Initial pass - set the determined widths
        SetDeterminedWidths(totalSize, ref flexibles);

        var remainderPercentage = 1f - _resolvedArray.Sum();
        
        // Second pass - Set min or max widths if they are determined
        float minmaxPercentage = 0;
        do
        {
            minmaxPercentage = remainderPercentage / flexibles.Count;
            SetMinMax(totalSize, ref flexibles, minmaxPercentage);
        }
        // If our minmaxPercentage has changed, we need another pass, as some elements might now be blocked
        while (flexibles.Count > 0 && !minmaxPercentage.LossyEquals(remainderPercentage / flexibles.Count));
        
        // Third pass - Just distribute the remainder
        if (flexibles.Count > 0)
        {
            remainderPercentage = 1f - _resolvedArray.Sum();
            float distribution = remainderPercentage / flexibles.Count;
            foreach (var index in flexibles)
                _resolvedArray[index] = distribution;
        }
        
        // Fourth pass - transform % to actual widths
        var total = _resolvedArray.Sum(); // Divide by sum instead of 1, as it may exceed 1
        for (var i = 0; i < _resolvedArray.Length; i++)
        {
            float actualPercentage = (_resolvedArray[i] / total);
            _resolvedArray[i] = actualPercentage * totalSize;
        }
    }

    private void SetMinMax(float totalSize, ref List<int> flexibles, float minmaxPercentage)
    {
        for (var i = flexibles.Count - 1; i >= 0; i--)
        {
            var index = flexibles[i];
            var target = this[index];
            if (target.MinSize > 0f)
            {
                var targetMinWidth = ResolveSize(target.MinSize, totalSize);
                // If the amount of % i would get is smaller than what i want, i am determined
                if (targetMinWidth > minmaxPercentage)
                {
                    _resolvedArray[index] = targetMinWidth;
                    flexibles.RemoveAt(i);
                }
            }

            if (target.MaxSize > 0f)
            {
                var targetMinWidth = ResolveSize(target.MaxSize, totalSize);
                // If the amount of % i would get is larger than what i want, i am determined
                if (targetMinWidth < minmaxPercentage)
                {
                    _resolvedArray[index] = targetMinWidth;
                    flexibles.RemoveAt(i);
                }
            }
        }
    }

    private void SetDeterminedWidths(float totalSize, ref List<int> flexibles)
    {
        for (var i = flexibles.Count - 1; i >= 0; i--)
        {
            var index = flexibles[i];
            var target = this[index];
            
            if (!(target.PreferredSize > 0f)) continue;
            
            // If we have a preferredSize -> set it
            _resolvedArray[index] = ResolveSize(target.PreferredSize, totalSize);
            flexibles.RemoveAt(i);
        }
    }

    private float ResolveSize(float size, float total)
    {
        if (size > 1f) // transform non-percentage widths to percentages
            size /= total;
        return size;
    }

    public void ClearCache()
    {
        _resolvedSize = 0;
    }
}
