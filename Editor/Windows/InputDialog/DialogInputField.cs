using UnityEditor;
using UnityEngine;

public abstract class DialogInputField
{
    protected GUIContent _label;
    public GUIContent Label => _label;

    public abstract object WeakValue { get; }
    public int Height { get; protected set; } = 22;

    public delegate void InputFieldHandler(DialogInputField field);

    public event InputFieldHandler ValidateValue;
    public event InputFieldHandler ValueChanged;

    public void Draw(GUIContent label)
    {
        DrawField(label ?? Label);
    }
    
    public virtual int GetWidth()
    {
        return 0;
    }
    
    private void DrawField(GUIContent label)
    {
        var rect = EditorGUILayout.GetControlRect();
        if (label != null)
        {
#if ODIN_INSPECTOR
            var labelWidth = Sirenix.Utilities.Editor.GUIHelper.BetterLabelWidth;
#else
            var labelWidth = EditorGUIUtility.labelWidth;
#endif
            var labelRect = AlignLeft(rect, labelWidth);
            var fieldRect = AlignRight(rect, rect.width - labelWidth);

            EditorGUI.LabelField(labelRect, label);
            DrawFieldValue(fieldRect);
        }
        else
            DrawFieldValue(rect);
    }
    
    public static Rect AlignLeft(Rect rect, float width)
    {
        rect.width = width;
        return rect;
    }
    
    public static Rect AlignRight(Rect rect, float width)
    {
        rect.x = rect.x + rect.width - width;
        rect.width = width;
        return rect;
    }

    protected abstract void DrawFieldValue(Rect rect);
}

public abstract class DialogInputField<T> : DialogInputField
{
    public T SmartValue;

    public T Get() => SmartValue;

    protected DialogInputField(string label, string tooltip = null, T initialValue = default(T))
    {
        _label = new GUIContent(label, tooltip);
        SmartValue = initialValue;
    }

    public override object WeakValue => SmartValue;
}
