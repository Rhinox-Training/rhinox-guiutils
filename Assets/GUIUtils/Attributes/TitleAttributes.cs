using System;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Attributes
{
    [IncludeMyAttributes]
    [DontApplyToListElements]
    [Title("@$property.ValueEntry.TypeOfValue.Name")]
    public class ValueTypeAsTitleAttribute : Attribute
    {
    }
    
    [IncludeMyAttributes]
    [DontApplyToListElements]
    [Title("@$property.ParentType.Name")]
    public class ParentTypeAsTitleAttribute : Attribute
    {
    }
}