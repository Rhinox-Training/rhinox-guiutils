using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GUIButton
    {
        public GUIContent Label;
        public Func<bool> _canExecute;
        public Action _action;
        
        public GUIButton(string label, Func<bool> canExecute, Action action, string tooltip = null)
            : this(new GUIContent(label, tooltip), canExecute, action)
        { }
        
        public GUIButton(Texture tex, Func<bool> canExecute, Action action, string tooltip = null)
            : this(new GUIContent(tex, tooltip), canExecute, action)
        { }
        
        public GUIButton(Texture tex, string label, Func<bool> canExecute, Action action, string tooltip = null)
            : this(new GUIContent(label, tex, tooltip), canExecute, action)
        { }
        
        public GUIButton(GUIContent label, Func<bool> canExecute, Action action)
        {
            Label = label;
            
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