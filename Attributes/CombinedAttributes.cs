using System;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Attributes
{
    // Note: IncludeMyAttributes will NOT work on classes
    [IncludeMyAttributes]
    [ShowInInspector, HideInPlayMode]
    public class ShowInEditorAttribute : Attribute
    {
    }
    
    [IncludeMyAttributes]
    [ShowInInspector, HideInEditorMode]
    public class ShowInPlayModeAttribute : Attribute
    {
    }
    
    [IncludeMyAttributes]
    [ShowInInspector, ReadOnly]
    public class ShowReadOnlyAttribute : Attribute
    {
    }

    [IncludeMyAttributes]
    [ShowInInspector, ReadOnly, HideInPlayMode]
    public class ShowReadOnlyInEditorAttribute : Attribute
    {
    }
    
    [IncludeMyAttributes]
    [ShowInInspector, ReadOnly, HideInEditorMode]
    public class ShowReadOnlyInPlayModeAttribute : Attribute
    {
    }
    
    [IncludeMyAttributes]
    [HideLabel, HideReferenceObjectPicker, HideDuplicateReferenceBox]
    [Obsolete("Does not work - Code seems to check for HideReferenceObjectPicker directly before resolving this.")]
    public class HideReferencePickerCompletelyAttribute : Attribute
    {
    }
}
