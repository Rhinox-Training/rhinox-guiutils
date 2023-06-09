using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.GUIUtils.Utils
{
    public static class LayerUtils
    {
        public static string[] GetLayerNames()
        {
            return Enumerable.Range(0, 32)
                             .Select(index => LayerMask.LayerToName(index))
                             .Where(l => !string.IsNullOrEmpty(l))
                             .ToArray();
        }
    }
}