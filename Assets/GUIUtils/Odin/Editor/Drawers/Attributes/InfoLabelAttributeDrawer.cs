using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[DrawerPriority(DrawerPriorityLevel.AttributePriority)]
public class InfoLabelAttributeDrawer : OdinAttributeDrawer<InfoLabelAttribute>
{
    private PropertyMemberHelper<string> _text;

    private Rect _cachedRect;
    private GUIContent _content;
    private GUIContent _iconContent;

    protected override void Initialize()
    {
        _text = new PropertyMemberHelper<string>(this.Property, this.Attribute.Text);
        var info = _text.GetValue();
        _content = new GUIContent(info);
        _iconContent = new GUIContent(image: EditorIcons.Info.Inactive, tooltip: info);
    }
    
    protected override void DrawPropertyLayout(GUIContent label)
    {
        CallNextDrawer(label);

        var style = SirenixGUIStyles.RightAlignedGreyMiniLabel;
        GUI.skin.label.CalcMinMaxWidth(label, out _, out float max);
        style.CalcMinMaxWidth(_content, out _, out float maxText);

        var labelRect = GUILayoutUtility.GetLastRect();
        if (labelRect.width > 1)
        {
            _cachedRect = labelRect;
            _cachedRect.width = EditorGUIUtility.labelWidth;
            _cachedRect.AddXMin(max);
        }
        
        if (Attribute.AlwaysShowAsTooltip || _cachedRect.width < max + maxText)
            GUI.Label(_cachedRect, _iconContent, style);
        else
            GUI.Label(_cachedRect, _content, style);
    }
}
