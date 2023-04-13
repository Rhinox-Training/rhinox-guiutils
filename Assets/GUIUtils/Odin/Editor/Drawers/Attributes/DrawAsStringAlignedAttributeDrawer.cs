using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class DrawAsStringAlignedAttributeDrawer<T> : OdinAttributeDrawer<DisplayAsStringAlignedAttribute, T>
    {
        private GUIStyle _currentLabelStyle;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            IPropertyValueEntry<T> valueEntry = this.ValueEntry;
            DisplayAsStringAlignedAttribute attribute = this.Attribute;
            if (valueEntry.Property.ChildResolver is ICollectionResolver)
            {
                this.CallNextDrawer(label);
            }
            else
            {
                string str = (object) valueEntry.SmartValue == null ? "Null" : valueEntry.SmartValue.ToString();
                if (label == null)
                    EditorGUILayout.LabelField(str, GetStyle(Attribute.Alignment), (GUILayoutOption[]) GUILayoutOptions.MinWidth(0.0f));
                else if (!attribute.Overflow)
                {
                    GUIContent content = GUIHelper.TempContent(str);
                    GUI.Label(
                        EditorGUI.PrefixLabel(
                            EditorGUILayout.GetControlRect(false, 
                                SirenixGUIStyles.MultiLineLabel.CalcHeight(content, valueEntry.Property.LastDrawnValueRect.width - GUIHelper.BetterLabelWidth), 
                                (GUILayoutOption[]) GUILayoutOptions.MinWidth(0.0f)), label), 
                        content, 
                        GetStyle(Attribute.Alignment));
                }
                else
                {
                    Rect valueRect;
                    SirenixEditorGUI.GetFeatureRichControlRect(label, out int _, out bool _, out valueRect);
                    GUI.Label(valueRect, str, GetStyle(Attribute.Alignment));
                }
            }
        }
        
        private GUIStyle GetStyle(TextAlignment alignment)
        {
            if (_currentLabelStyle == null)
            {
                GUIStyle labelStyle = Attribute.Overflow ? CustomGUIStyles.Label : SirenixGUIStyles.MultiLineLabel;
                labelStyle = new GUIStyle(labelStyle);
                switch (alignment)
                {
                    case TextAlignment.Center:
                        labelStyle.alignment = TextAnchor.MiddleCenter;
                        break;
                    case TextAlignment.Right:
                        labelStyle.alignment = TextAnchor.MiddleRight;
                        break;
                }
                _currentLabelStyle = labelStyle;
            }

            return _currentLabelStyle;
        }
    }
}