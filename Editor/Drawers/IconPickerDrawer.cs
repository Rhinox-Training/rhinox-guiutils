using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(IconPickerAttribute))]
    public class IconPickerDrawer : BasePickerPropertyDrawer<Texture, UnityIcon>
    {
        protected override BasePicker BuildPicker(DrawerData data)
        {
            var icons = new List<UnityIcon>();
            UnityIconFinder.FindIcons(ref icons);

            var typedCollection = new TypedFilteredCollection<UnityIcon>(
                icons,
                x => x.Name,
                x => x.Origin,
                x => x.Icon
            );
            return new SimplePicker<UnityIcon>(typedCollection);
        }

        protected override UnityIcon ReverseLookup(Texture fieldVal)
        {
            var icons = new List<UnityIcon>();
            UnityIconFinder.FindIcons(ref icons);
            return icons.FirstOrDefault(x => x.Icon == fieldVal);
        }

        protected override string GetNameForSelection(UnityIcon pickerVal) => pickerVal.Name;

        protected override Texture GetValueForSelection(UnityIcon pickerVal) => pickerVal.Icon;
    }
}