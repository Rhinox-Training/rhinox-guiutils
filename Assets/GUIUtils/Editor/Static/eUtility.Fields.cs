using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static partial class eUtility
    {
        public static LayerMask LayerMaskField(GUIContent label, LayerMask layerMask)
        {
            string[] layers = InternalEditorUtility.layers;

            var mask = GetMaskValue(layerMask, layers);
                
            mask = EditorGUILayout.MaskField(label, mask, layers);
            
            return ConvertMaskValue(mask);
        }
        
        public static LayerMask LayerMaskField(Rect rect, GUIContent label, LayerMask layerMask)
        {
            string[] layers = InternalEditorUtility.layers;

            var mask = GetMaskValue(layerMask, layers);

            mask = EditorGUI.MaskField(rect, label, mask, layers);
            
            return ConvertMaskValue(mask);
        }
        
        private static readonly List<int> _layerNumbers = new List<int>();

        // InternalEditorUtility.LayerMaskToConcatenatedLayersMask but without empty entries
        private static int GetMaskValue(LayerMask layerMask, string[] layers)
        {
            _layerNumbers.Clear();
            
            for (int i = 0; i < layers.Length; i++)
                _layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

            int mask = 0;
            for (int i = 0; i < _layerNumbers.Count; i++)
            {
                if (((1 << _layerNumbers[i]) & layerMask.value) > 0)
                    mask |= 1 << i;
            }

            return mask;
        }
        
        // InternalEditorUtility.ConcatenatedLayersMaskToLayerMask
        private static int ConvertMaskValue(int maskWithoutEmpty)
        {
            int mask = 0;
            for (int i = 0; i < _layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= 1 << _layerNumbers[i];
            }

            return mask;
        }

        public static int TagMaskField(GUIContent label, int tagMask)
        {
            string[] tags = InternalEditorUtility.tags;

            int maskWithoutEmpty = 0;
            for (int i = 0; i < tags.Length; i++)
            {
                if (((1 << i) & tagMask) > 0)
                    maskWithoutEmpty |= 1 << i;
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, tags);

            int mask = 0;
            for (int i = 0; i < tags.Length; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= 1 << i;
            }

            tagMask = mask;

            return tagMask;
        }
    }
}