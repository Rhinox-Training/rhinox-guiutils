using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class LayerMaskDrawableField: BaseMemberDrawable<LayerMask>
    {
        public LayerMaskDrawableField(GenericHostInfo hostInfo) : base(hostInfo) { }

        protected override LayerMask DrawValue(GUIContent label, LayerMask value, params GUILayoutOption[] options)
        {
            return eUtility.LayerMaskField(label, value);
        }

        protected override LayerMask DrawValue(Rect rect, GUIContent label, LayerMask value)
        {
            return eUtility.LayerMaskField(rect, label, value);
        }
    }
}