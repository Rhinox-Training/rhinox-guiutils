#if ODIN_INSPECTOR
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
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
        }

    }
}

#endif