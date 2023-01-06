using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Rhinox.GUIUtils.Editor
{
    public class DialogBuilder
    {
        public interface IValueReference
        {
            object GenericValue { get; }
        }
        
        public abstract class ValueReference<T> : IValueReference
        {
            public abstract T Value { get; }

            public object GenericValue => Value;

            public static implicit operator T(ValueReference<T> reference) => reference.Value;
        }

        public class FieldWrapper<T> : ValueReference<T>
        {
            public DialogInputField<T> Field;

            public FieldWrapper(DialogInputField<T> field)
            {
                Field = field;
            }

            public override T Value => Field.SmartValue;
        }

        public class ValueDropdownItemWrapper<T> : ValueReference<T>
        {
            public DropdownInputField<T> Field;
            
            public ValueDropdownItemWrapper(DropdownInputField<T> field)
            {
                Field = field;
            }

            public override T Value => Field.SmartValue.Value;
        }

        private DialogData _dialogData;

        public DialogBuilder(string title, string content, string confirm = "Confirm", string cancel = "Cancel")
        {
            _dialogData = new DialogData(title, content, confirm, cancel);
        }

        public void ConfirmMessage(string confirm) => _dialogData.ConfirmButton = confirm;
        public void CancelMessage(string cancel) => _dialogData.CancelButton = cancel;

        private DialogBuilder Add<T, TValue>(T field, out ValueReference<TValue> reference, TValue initialValue)
            where T : DialogInputField<TValue>
        {
            reference = new FieldWrapper<TValue>(_dialogData.Add(field, initialValue));
            return this;
        }

        public DialogBuilder BooleanField(string name, out ValueReference<bool> reference, bool initialValue = default,
            string tooltip = null)
        {
            var field = new BoolInputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder IntField(string name, out ValueReference<int> reference, int initialValue = default,
            string tooltip = null)
        {
            var field = new Int32InputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder FloatField(string name, out ValueReference<float> reference, float initialValue = default,
            string tooltip = null)
        {
            var field = new FloatInputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder TextField(string name, out ValueReference<string> reference, string initialValue = "",
            string tooltip = null)
        {
            var field = new TextInputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }
        
        public DialogBuilder GenericUnityObjectField<T>(string name, out ValueReference<T> reference,
            T initialValue = null, string tooltip = null) where T : UnityEngine.Object
        {
            var field = new GenericUnityObjectInputField<T>(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder TransformField(string name, out ValueReference<Transform> reference,
            Transform initialValue = null, string tooltip = null)
        {
            var field = new TransformInputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder GameObjectField(string name, out ValueReference<GameObject> reference,
            GameObject initialValue = null, string tooltip = null)
        {
            var field = new GameObjectInputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder MaterialField(string name, out ValueReference<Material> reference,
            Material initialValue = null, string tooltip = null)
        {
            var field = new MaterialInputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder TextureField(string name, out ValueReference<Texture> reference,
            Texture initialValue = null, string tooltip = null)
        {
            var field = new TextureInputField(name, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder Dropdown<T>(string name, ICollection<T> options, Func<T, string> nameSelector, out ValueReference<T> reference, T initialValue
     = default, string tooltip = null)
        {
            var valueOptions = new ValueDropdownList<T>();
            foreach (var val in options)
                valueOptions.Add(nameSelector(val), val);
            return Dropdown(name, valueOptions, out reference, initialValue, tooltip);
        }
        
        public DialogBuilder Dropdown<T>(string name, ICollection<ValueDropdownItem<T>> options, out ValueReference<T> reference, ValueDropdownItem<T> initialValue
     = default, string tooltip = null)
        {
            var field = new DropdownInputField<T>(name, options, tooltip);
            _dialogData.Add(field, initialValue);
            reference = new ValueDropdownItemWrapper<T>(field);
            return this;
        }
        
        public DialogBuilder Dropdown<T>(string name, ICollection<ValueDropdownItem<T>> options, out ValueReference<T> reference, T initialValue, string tooltip
     = null)
        {
            var initialPick = options.FirstOrDefault(x => Equals(x.Value, initialValue));
            return Dropdown(name, options, out reference, initialPick, tooltip);
        }
        
        public DialogBuilder Dropdown(string name, ICollection<ValueDropdownItem> options, out ValueReference<ValueDropdownItem> reference, ValueDropdownItem initialValue
     = default, string tooltip = null)
        {
            var field = new DropdownInputField(name, options, tooltip);
            return Add(field, out reference, initialValue);
        }

        public DialogBuilder OnAccept(Action action)
        {
            _dialogData.Accepted += action;
            return this;
        }

        public DialogBuilder OnCancel(Action action)
        {
            _dialogData.Canceled += action;
            return this;
        }

        public EditorInputDialog Show()
        {
            return EditorInputDialog.ShowDialog(_dialogData);
        }

        public EditorInputDialog ShowInPopup()
        {
            return EditorInputDialog.ShowInPopup(_dialogData);
        }

        public EditorInputDialog ShowBlocking()
        {
            return EditorInputDialog.ShowBlocking(_dialogData);
        }
    }

    public class DialogData
    {
        public string Title;
        public string Content;
        public string ConfirmButton;
        public string CancelButton;

        private List<DialogInputField> _fields;

        public event Action Accepted;
        public event Action Canceled;

        public IReadOnlyList<DialogInputField> Fields => _fields.AsReadOnly();

        private DialogData()
        {
            _fields = new List<DialogInputField>();
        }

        public DialogData(string title, string content, string confirm = "Confirm", string cancel = "Cancel")
            : this()
        {
            Title = title;
            Content = content;
            ConfirmButton = confirm;
            CancelButton = cancel;
        }

        public DialogInputField<T> Add<T>(DialogInputField<T> field, T initialValue = default)
        {
            field.SmartValue = initialValue;
            _fields.Add(field);
            return field;
        }

        public void Resolve(bool accepted)
        {
            if (accepted)
                Accepted?.Invoke();
            else
                Canceled?.Invoke();
        }

        public float GetPreferredWidth()
        {
            float width = 0;
            foreach (var field in Fields)
                width = Mathf.Max(field.GetWidth(), width);
            return width;
        }

        public float GetPreferredHeight()
        {
            float height = Fields.Sum(x => x.Height);
            height += EditorGUIUtility.singleLineHeight; // title
            height += EditorGUIUtility.singleLineHeight; // buttons
            height += 5; // padding
            return height;
        }
        
        public static DialogBuilder Create(string title, string content, string confirm = "Confirm", string cancel = "Cancel")
        {
            return new DialogBuilder(title, content, confirm, cancel);
        }
    }
}