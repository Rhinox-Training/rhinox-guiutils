using System;
using System.Collections;
using System.Reflection;
using Rhinox.GUIUtils;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0, 1000, 0)]
    public class UnfoldListAttributeDrawer : OdinAttributeDrawer<UnfoldListAttribute>
    {
        private string _errorMessage;
        private Action<object> _onBeforeTitleGUI;
        private Action<object> _onAfterTitleGUI;
        
        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ValueEntry.TypeOfValue.InheritsFrom(typeof(IEnumerable));
        }

        protected override void Initialize()
        {
            _onBeforeTitleGUI = GetMethodInvoker(Attribute.OnBeforeTitleGUI, ref _errorMessage);
            _onAfterTitleGUI = GetMethodInvoker(Attribute.OnAfterTitleGUI, ref _errorMessage);

            base.Initialize();
        }

        private Action<object> GetMethodInvoker(string methodName, ref string errorMessage)
        {
            if (methodName.IsNullOrWhitespace() || !errorMessage.IsNullOrWhitespace()) return null;
            
            MemberInfo member = Property.ParentType.FindMember().IsMethod().IsNamed(methodName)
                .HasNoParameters().ReturnsVoid().GetMember<MethodInfo>(out errorMessage);
            
            if (member != null && errorMessage == null)
                return EmitUtilities.CreateWeakInstanceMethodCaller(member as MethodInfo);
            
            errorMessage = errorMessage ?? "There should really be an error message here.";
            return null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!_errorMessage.IsNullOrWhitespace())
                SirenixEditorGUI.ErrorMessageBox(_errorMessage);
            
            SirenixEditorGUI.BeginVerticalList(true, true);
            
            GUILayout.BeginHorizontal();

            _onBeforeTitleGUI?.Invoke(Property.ParentValues[0]);

            if (label != null)
            {
                GUILayout.BeginVertical();
                
                SirenixEditorGUI.BeginListItem(false, SirenixGUIStyles.BoxHeaderStyle);
                GUILayout.Label(label, GetStyle());
                SirenixEditorGUI.EndListItem();
                
                GUILayout.Space(1);
                
                GUILayout.EndVertical();
            }

            _onAfterTitleGUI?.Invoke(Property.ParentValues[0]);

            GUILayout.EndHorizontal();

            foreach (var child in Property.Children)
            {
                SirenixEditorGUI.BeginListItem();
                child.Draw(null);
                SirenixEditorGUI.EndListItem();
            }
            
            SirenixEditorGUI.EndVerticalList();
        }

        private GUIStyle GetStyle()
        {
            switch (Attribute.LabelAlignment)
            {
                case TextAlignment.Right: return SirenixGUIStyles.BoldTitleRight;
                case TextAlignment.Center: return SirenixGUIStyles.BoldTitleCentered;
                case TextAlignment.Left: return SirenixGUIStyles.BoldTitle;
                default: return SirenixGUIStyles.BoldTitle;
            }
        }
    }
}