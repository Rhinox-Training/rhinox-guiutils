using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public class InlineEditorIconAttributeDrawer<T> : OdinAttributeDrawer<InlineEditorIconAttribute, T>
    {
        protected EditorIcon _icon;

        private ButtonContext _context;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            var t = typeof(EditorIcons);
            _icon = t.GetProperty(Attribute.EditorIcon, Flags.StaticPublic)?.GetValue(null) as EditorIcon;
        }
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            CheckPropertyContext();

            if (_context.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(_context.ErrorMessage, true);
                this.CallNextDrawer(label);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.BeginVertical();
                this.CallNextDrawer(label);
                EditorGUILayout.EndVertical();
                
                if (SirenixEditorGUI.IconButton(_icon, tooltip: Attribute.Tooltip))
                {
                    if (_context.StaticMethodCaller != null)
                        _context.StaticMethodCaller();
                    else if (_context.InstanceMethodCaller != null)
                        _context.InstanceMethodCaller(ValueEntry.Property.ParentValues[0]);
                    else if (_context.InstanceParameterMethodCaller != null)
                        _context.InstanceParameterMethodCaller(ValueEntry.Property.ParentValues[0], ValueEntry.SmartValue);
                    else // Should never reach here? This would be caught by ErrorMessage
                        Debug.LogError("No method found.");
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void CheckPropertyContext()
        {
            if (_context != null) return;
            
            _context = new ButtonContext();
            if (_context.ErrorMessage != null) return;
            
            MethodInfo memberInfo;
            
            // Find method with no params
            if (MemberFinder.Start(ValueEntry.ParentType).IsMethod().IsNamed(Attribute.MemberMethod).HasNoParameters()
                .TryGetMember(out memberInfo, out _context.ErrorMessage))
            {
                if (memberInfo.IsStatic())
                    _context.StaticMethodCaller = EmitUtilities.CreateStaticMethodCaller(memberInfo);
                else
                    _context.InstanceMethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller(memberInfo);
            }
            // Find method taking an instance of the value
            else if (MemberFinder.Start(ValueEntry.ParentType).IsMethod().IsNamed(Attribute.MemberMethod).HasParameters<T>()
                .TryGetMember(out memberInfo, out _context.ErrorMessage))
            {
                if (memberInfo.IsStatic())
                    _context.ErrorMessage = "Static parameterized method is currently not supported.";
                else
                    _context.InstanceParameterMethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller<T>(memberInfo);
            }
        }

        private class ButtonContext
        {
            public string ErrorMessage;
            public Action StaticMethodCaller;
            public Action<object> InstanceMethodCaller;
            public Action<object, T> InstanceParameterMethodCaller;
        }
    }
}