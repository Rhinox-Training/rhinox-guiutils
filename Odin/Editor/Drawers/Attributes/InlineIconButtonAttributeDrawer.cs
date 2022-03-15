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
    public class InlineIconButtonAttributeDrawer<T> : OdinAttributeDrawer<InlineIconButtonAttribute, T>
    {
        protected class ButtonContext
        {
            public string ErrorMessage;

            // public StringMemberHelper LabelHelper;
            public Action StaticMethodCaller;
            public Action<object> InstanceMethodCaller;
            public Action<object, T> InstanceParameterMethodCaller;
        }

        private EditorIcon _icon;
        protected ButtonContext _context;

        /// <summary>Initializes the drawer.</summary>
        protected override void Initialize()
        {
            const BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public;
            var t = typeof(EditorIcons);

            _icon = t.GetProperty(Attribute.Icon, bindingAttr)?.GetValue(null) as EditorIcon;
        }

        protected void CheckMethodContext()
        {
            // If value already exists; we already looked for it and the context should be OK
            if (_context != null) return;

            _context = new ButtonContext();

            MethodInfo memberInfo;
            // Handle no param method
            if (MemberFinder.Start(ValueEntry.ParentType)
                .IsMethod()
                .IsNamed(Attribute.MethodName)
                .HasNoParameters()
                .TryGetMember<MethodInfo>(out memberInfo, out _context.ErrorMessage))
            {
                if (memberInfo.IsStatic())
                    _context.StaticMethodCaller = EmitUtilities.CreateStaticMethodCaller(memberInfo);
                else
                    _context.InstanceMethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller(memberInfo);
            }
            // Handle parameter method
            else if (MemberFinder
                .Start(ValueEntry.ParentType)
                .IsMethod()
                .IsNamed(Attribute.MethodName)
                .HasParameters<T>()
                .TryGetMember<MethodInfo>(out memberInfo, out _context.ErrorMessage))
            {
                if (memberInfo.IsStatic())
                    _context.ErrorMessage = "Static parameterized method is currently not supported.";
                else
                    _context.InstanceParameterMethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller<T>(memberInfo);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_icon == null)
            {
                SirenixEditorGUI.ErrorMessageBox($"Cannot find icon '{Attribute.Icon}'");
                _icon = EditorIcons.X;
            }

            CheckMethodContext();

            if (_context.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(_context.ErrorMessage);
                this.CallNextDrawer(label);
            }

            // Draw button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            this.CallNextDrawer(label);
            EditorGUILayout.EndVertical();

            if (Attribute.ForceEnable)
                GUIHelper.PushGUIEnabled(true);

            if (SirenixEditorGUI.IconButton(_icon, tooltip: Attribute.Tooltip))
            {
                // Invoke the method
                if (_context.StaticMethodCaller != null)
                    _context.StaticMethodCaller();
                else if (_context.InstanceMethodCaller != null)
                    _context.InstanceMethodCaller(ValueEntry.Property.ParentValues[0]);
                else if (_context.InstanceParameterMethodCaller != null)
                    _context.InstanceParameterMethodCaller(ValueEntry.Property.ParentValues[0], ValueEntry.SmartValue);
                else
                    Debug.LogError((object) "No method found.");
            }

            if (Attribute.ForceEnable)
                GUIHelper.PopGUIEnabled();

            EditorGUILayout.EndHorizontal();
        }
    }
}