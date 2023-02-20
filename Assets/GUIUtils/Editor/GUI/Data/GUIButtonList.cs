using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GUIButtonList : List<GUIButton>
    {
        private int _buttonsDrawn;

        public GUIButtonList(IEnumerable<GUIButton> collection)
            : base(collection)  
        {
        }

        public void Draw(bool hideDisabled, params GUILayoutOption[] options)
        {
            foreach (var button in this)
            {
                var canExecute = button.CanExecute();
                if (!canExecute && hideDisabled)
                    continue;

                using (new eUtility.DisabledGroup(!canExecute))
                {
                    if (GUILayout.Button(button.Label, options))
                        button.Execute();
                }
            }
        }
        
        public void DrawHorizontal(bool hideDisabled, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            int buttonsDrawn = 0;
            for (var i = 0; i < this.Count; i++)
            {
                var button = this[i];
                var canExecute = button.CanExecute();
                if (!canExecute && hideDisabled)
                    continue;

                using (new eUtility.DisabledGroup(!canExecute))
                {
                    if (GUILayout.Button(button.Label, CustomGUIStyles.GetButtonGroupStyle(i, _buttonsDrawn), options))
                        button.Execute();
                    ++buttonsDrawn;
                }
            }
            GUILayout.EndHorizontal();
            // _buttonsDrawn is cached and thus the info of the previous iteration
            // but due to the layout/repaint loop; this is never visible
            _buttonsDrawn = buttonsDrawn;
        }
    }
}