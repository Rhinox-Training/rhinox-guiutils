using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DropdownBaseWrapper : BaseWrapperDrawable
    {
        private IPropertyMemberHelper<IEnumerable> _member;
        
        public override float ElementHeight => EditorGUIUtility.singleLineHeight;
        
        private GUIContent _activeItem;

        public GUIContent ActiveItem
        {
            get => _activeItem;
            set
            {
                _activeItem = value;
                _valueChanged = true;
                _frameHandler = new NewFrameHandler();
            }
        }
        
        private readonly GUIContent _defaultItem = new GUIContent("<None>");

        private Rect _dropdownRect;
        private bool _valueChanged;
        private NewFrameHandler _frameHandler;

        public DropdownBaseWrapper(IOrderedDrawable drawable, IPropertyMemberHelper<IEnumerable> member) : base(drawable)
        {
            _member = member;
            _activeItem = FindActiveItem() ?? _defaultItem;
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            OnPreDraw();
            
            _member.DrawError();

            EditorGUILayout.BeginHorizontal(options);
            EditorGUILayout.PrefixLabel(label);

            var clicked = EditorGUILayout.DropdownButton(ActiveItem, FocusType.Keyboard, options);
            
            var rect = GUILayoutUtility.GetLastRect();
            if (rect.IsValid())
                _dropdownRect = rect;

            if (clicked)
                MakeMenuItems(_dropdownRect);
            
            EditorGUILayout.EndHorizontal();
            
            OnPostDraw();
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            OnPreDraw();

            rect = EditorGUI.PrefixLabel(rect, label);
            
            if (EditorGUI.DropdownButton(rect, ActiveItem, FocusType.Keyboard))
                MakeMenuItems(rect);
            
            _member.DrawError(rect);

            OnPostDraw();
        }

        protected override void OnPostDraw()
        {
            base.OnPostDraw();
            
            if (_valueChanged)
            {
                GUI.changed = true;
                _valueChanged = !_frameHandler.IsNewFrame();
            }
        }

        private void MakeMenuItems(Rect rect)
        {
            object[] options = Array.Empty<object>();
            var value = _member.ForceGetValue();
            if (value != null)
                options = value.Cast<object>().ToArray();
            
            var menu = new GenericMenu();

            foreach (var item in options)
            {
                if (item is IValueDropdownItem dropdownItem)
                {
                    var text = dropdownItem.GetText();
                    menu.AddItem(text, () =>
                    {
                        SetValue(dropdownItem.GetValue());
                        ActiveItem = new GUIContent(text);
                    });
                }
                else
                {
                    
                    var text = item.ToString();
                    menu.AddItem(text, () =>
                    {
                        SetValue(item);
                        ActiveItem = new GUIContent(text);
                    });
                }
            }

            if (!options.Any())
                menu.AddDisabledItem(new GUIContent("No items..."), false);
            
            menu.DropDown(rect);
        }
        
        private GUIContent FindActiveItem()
        {
            var currentVal = GetValue();
            if (currentVal == null)
                return null;
            var value = _member.ForceGetValue();
            if (value == null)
                return null;
            
            var options = value.Cast<object>().ToArray();
            foreach (var item in options)
            {
                var itemVal = item;
                if (itemVal is IValueDropdownItem dropdownItem)
                    itemVal = dropdownItem.GetValue();

                if (!object.Equals(itemVal, currentVal))
                    continue;
                
                string stringText = null;
                if (item is IValueDropdownItem dropdownItem2)
                    stringText = dropdownItem2.GetText();
                else
                    stringText = itemVal.ToString();
                
                return new GUIContent(stringText);
            }

            return null;
        }

        [WrapDrawer(typeof(ValueDropdownAttribute), -1)]
        public static BaseWrapperDrawable Create(ValueDropdownAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<IEnumerable>(drawable.HostInfo, attr.MemberName);
            return new DropdownBaseWrapper(drawable, member);
        }
    }
}