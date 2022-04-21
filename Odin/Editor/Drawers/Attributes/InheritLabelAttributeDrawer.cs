using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class InheritLabelAttributeDrawer : OdinAttributeDrawer<InheritLabelAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        label = Property.Parent.Label;
        this.CallNextDrawer(label);
    }
}
