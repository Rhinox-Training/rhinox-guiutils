using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DropdownInputField : DialogInputField<ValueDropdownItem>
    {
        private ValueDropdownItem[] _options;

        public DropdownInputField(string label, IEnumerable<ValueDropdownItem> options, string tooltip = null,
            ValueDropdownItem initialValue = default)
            : base(label, tooltip, initialValue)
        {
            _options = options.ToArray();
        }

        protected override void DrawFieldValue(Rect rect)
        {
#if ODIN_INSPECTOR
            if (GUI.Button(rect, SmartValue.Text, EditorStyles.miniPullDown))
            {
                var selector = new GenericSelector<ValueDropdownItem>(_options) { FlattenedTree = true };

                selector.EnableSingleClickToSelect();
                selector.SelectionConfirmed += x =>
                {
                    var t = x.FirstOrDefault();
                    SmartValue = t;
                };

                selector.ShowInPopup(rect);
            }
#else
            if (EditorGUI.DropdownButton(rect, GUIContentHelper.TempContent(SmartValue.Text ?? "<Select a value>", _tooltip), FocusType.Passive, EditorStyles.miniPullDown))
            {
                GenericPicker.Show(rect, _options, (x) => { SmartValue = x; },
                    (option) => option.Text); 
            }
#endif
        }

        public override float GetWidth()
        {
            return 200 + _options.Max(x => x.Text.Length) * 8;
        }

    }

    public class DropdownInputField<T> : DialogInputField<ValueDropdownItem<T>>
    {
        private ValueDropdownItem<T>[] _options;

        public DropdownInputField(string label, IEnumerable<ValueDropdownItem<T>> options, string tooltip = null,
            ValueDropdownItem<T> initialValue = default)
            : base(label, tooltip, initialValue)
        {
            _options = options.ToArray();
        }

        protected override void DrawFieldValue(Rect rect)
        {
#if ODIN_INSPECTOR
            if (GUI.Button(rect, GUIContentHelper.TempContent(SmartValue.Text, _tooltip), EditorStyles.miniPullDown))
            {
                var selector = new GenericSelector<ValueDropdownItem<T>>(_options) { FlattenedTree = true };

                selector.EnableSingleClickToSelect();
                selector.SelectionConfirmed += x =>
                {
                    var t = x.FirstOrDefault();
                    SmartValue = t;
                };

                selector.ShowInPopup(rect);
            }
#else
            if (EditorGUI.DropdownButton(rect, GUIContentHelper.TempContent(SmartValue.Text ?? "<Select a value>"), FocusType.Passive, EditorStyles.miniPullDown))
            {
                GenericPicker.Show(rect, _options, (x) => { SmartValue = x; },
                    (option) => option.Text); 
            }
#endif
        }
    }
}