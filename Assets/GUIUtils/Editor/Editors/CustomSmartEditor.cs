using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class CustomSmartEditor : BaseEditor<Object>
    {
        private DrawablePropertyView _propertyView;

        private static readonly Dictionary<Type, SmartFallbackDrawnAttribute> _attributeByType = new Dictionary<Type, SmartFallbackDrawnAttribute>();
        private static readonly Dictionary<Type, bool> _ignoreSmartFallbackByType = new Dictionary<Type, bool>();

        private static Texture2D _icon;

        private const string _iconTooltip =
            "Toggle Smart Editor\nNote: This is temporary, change the attribute if you wish a permanent change";

        [InitializeOnLoadMethod]
        private static void InitHeaderIcon()
        {
            var mi = typeof(CustomSmartEditor).GetMethod(nameof(DrawHeaderIcon), BindingFlags.NonPublic | BindingFlags.Static);
            ScriptEditorHeaderIcons.RegisterMethod(mi);
        }

        private static bool DrawHeaderIcon(Rect position, Object[] targets)
        {
            if (targets.Length != 1)
                return false;
            var type = targets[0].GetType();

            if (!_ignoreSmartFallbackByType.ContainsKey(type))
                return false;

            bool value = _ignoreSmartFallbackByType[type];
            Color col = GUI.color;
            if (value)
                GUI.color = Color.gray;
            
            if (CustomEditorGUI.IconButton(position, _icon, _iconTooltip, CustomGUIStyles.ToolbarIconButton))
                _ignoreSmartFallbackByType[type] = !value;

            GUI.color = col;
            return true;
        }

        protected override void OnEnable()
        {
            if (_icon == null)
                _icon = UnityIcon.AssetIcon("Fa_Magic");
            
            base.OnEnable();
            // Ensure if we're drawing a type, that it is included in the dictionary
            var type = target.GetType();
            if (!_attributeByType.ContainsKey(type))
            {
                var attr = type.GetCustomAttribute<SmartFallbackDrawnAttribute>();
                _attributeByType[type] = attr;
                _ignoreSmartFallbackByType[type] = attr == null;
            }
        }

        public override void OnInspectorGUI()
        {
            var type = target.GetType();
            if (_ignoreSmartFallbackByType.GetOrDefault(type))
            {
                base.OnInspectorGUI();
                return;
            }

            if (_attributeByType[type]?.AllowUnityIfAble == true)
            {
                int count = CountDrawnProperties(serializedObject);
                if (count > 0)
                {
                    base.OnInspectorGUI();
                    return;
                }
            }

            if (_propertyView == null)
            {
                _propertyView = new DrawablePropertyView(serializedObject);
                _propertyView.RepaintRequested += RequestRepaint;
            }
            
            DrawScriptField();
            _propertyView.DrawLayout();
        }
        
        private static int CountDrawnProperties(SerializedObject obj)
        {
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            int count = 0;
            while (iterator.NextVisible(enterChildren))
            {
                count++;
                enterChildren = false;
            }

            return count;
        }
    }
}