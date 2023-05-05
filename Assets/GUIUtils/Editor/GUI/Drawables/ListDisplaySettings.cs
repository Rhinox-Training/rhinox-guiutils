using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class ListDisplaySettings
    {
        public bool Expanded;
        public bool HideAddButton;
        public bool HideRemoveButton;
        public bool HideHeader;
        public bool DraggableItems;
        public bool ShowPaging;
        public int MaxItemsPerPage;
        public bool IsReadOnly;

        public ListDisplaySettings()
        {
            Expanded = true;
            HideAddButton = false;
            HideRemoveButton = false;
            HideHeader = false;
            DraggableItems = true;
            ShowPaging = false;
            MaxItemsPerPage = -1;
            IsReadOnly = false;
        }

        public ListDisplaySettings(ListDrawerSettingsAttribute attr)
        {
            Expanded = attr.Expanded;
            HideAddButton = attr.HideAddButton;
            HideRemoveButton = attr.HideRemoveButton;
            HideHeader = false;
            DraggableItems = attr.DraggableItems;
            ShowPaging = attr.ShowPaging;
            MaxItemsPerPage = attr.NumberOfItemsPerPage;
            IsReadOnly = attr.IsReadOnly;
        }
        
        public static ListDisplaySettings Create(GenericHostInfo hostInfo)
        {
            var foldoutAttr = hostInfo.GetAttribute<UnfoldListAttribute>();
            if (foldoutAttr != null)
            {
                return new ListDisplaySettings()
                {
                    Expanded = true,
                    HideAddButton = true,
                    HideRemoveButton = true,
                    HideHeader = true,
                    DraggableItems = false,
                    ShowPaging = false,
                };
            }

            var attr = hostInfo.GetAttribute<ListDrawerSettingsAttribute>();
            if (attr != null)
                return new ListDisplaySettings(attr);
            return new ListDisplaySettings();
        }
    }
}