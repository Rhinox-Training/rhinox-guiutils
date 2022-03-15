using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public abstract class InlineIconAttributeDrawer<T, TValue> : OdinAttributeDrawer<T, TValue> where T : Attribute
    {
        private GUIContent _guiContent;

        protected abstract Texture2D Icon { get; }
        protected abstract string Tooltip { get; }

        protected override void Initialize()
        {
            _guiContent = new GUIContent(Icon, Tooltip);
            base.Initialize();
        }

        protected void DrawWithIcon(GUIContent label)
        {
            using (new GUILayout.HorizontalScope(GUILayoutOptions.ExpandWidth(true)))
            {
                using (new GUILayout.VerticalScope(GUILayoutOptions.ExpandWidth(true)))
                {
                    this.CallNextDrawer(label);
                }

                GUILayout.Box(_guiContent, GUIStyle.none, GUILayoutOptions.Height(18).Width(18).ExpandWidth(false));
            }
        }
    }
}