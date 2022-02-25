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

            var list = new List<int>();

            for (int i = 0; i < layers.Length; i++)
                list.Add(LayerMask.NameToLayer(layers[i]));

            int maskWithoutEmpty = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (((1 << list[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= 1 << i;
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

            int mask = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= 1 << list[i];
            }

            layerMask.value = mask;

            return layerMask;
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