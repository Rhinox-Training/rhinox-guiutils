using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GUIButtonList : List<GUIButton>
    {
        private int _buttonsDrawn;

        public void Draw(bool hideDisabled)
        {
            foreach (var button in this)
            {
                var canExecute = button.CanExecute();
                if (!canExecute && hideDisabled)
                    continue;

                using (new eUtility.DisabledGroup(!canExecute))
                {
                    if (GUILayout.Button(button.Label))
                        button.Execute();
                }
            }
        }
        
        public void DrawHorizontal(bool hideDisabled)
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
                    if (GUILayout.Button(button.Label, CustomGUIStyles.GetButtonGroupStyle(i, _buttonsDrawn)))
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
    
    public class GUIButton
    {
        public GUIContent Label;
        public Func<bool> _canExecute;
        public Action _action;
        
        public GUIButton(string label, Func<bool> canExecute, Action action)
        {
            Label = new GUIContent(label);
            
            _canExecute = canExecute;
            _action = action;
        }

        public void Execute()
        {
            _action.Invoke();
        }
        
        public bool CanExecute() => _canExecute.Invoke();
    }
}