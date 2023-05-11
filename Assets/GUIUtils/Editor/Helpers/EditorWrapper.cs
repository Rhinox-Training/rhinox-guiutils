using System;
using Rhinox.GUIUtils.Editor;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if ODIN_INSPECTOR_3 && ODIN_VALIDATOR
using Sirenix.OdinValidator.Editor;
#endif

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Wrapper for an object to draw a certain object keeping editing capabilities
    /// </summary>
    public struct EditorWrapper : IEditor, IRepaintRequestHandler
    {
        [ShowIf(nameof(_expanded)), ShowInInspector]
        [InlineEditor(Expanded = true, ObjectFieldMode = InlineEditorObjectFieldModes.CompletelyHidden)]
        public object Target;
        
        private IRepaintable _repaintHandler;

#if ODIN_INSPECTOR
        private PropertyTree _tree;
#else
        private DrawablePropertyView _view;
#endif
        private bool _expanded;

        private static GUIStyle _headerStyle;
        public static GUIStyle HeaderStyle => _headerStyle ?? (_headerStyle = new GUIStyle(CustomGUIStyles.ToggleGroupBackground)
        {
            fixedHeight = 26
        });

        public Texture ClosedIcon
        {
            get
            {
                if (_closedIcon == null)
                    _closedIcon = UnityIcon.AssetIcon("Fa_AngleRight").MakeSquare().Pad(10);
                return _closedIcon;
            }
        }
        private Texture _closedIcon;
        
        public Texture OpenIcon
        {
            get
            {
                if (_openIcon == null)
                    _openIcon = UnityIcon.AssetIcon("Fa_AngleDown").MakeSquare().Pad(10);
                return _openIcon;
            }
        }
        private Texture _openIcon;

        public EditorWrapper(object target, bool expanded = true)
        {
            Target = target;
            _expanded = expanded;
            _closedIcon = null;
            _openIcon = null;
            _repaintHandler = null;
#if ODIN_INSPECTOR
            _tree = null;
#else
            _view = null;
#endif
        }

        public void DrawHeader()
        {
            if (Target == null)
                return;
            
            CustomEditorGUI.BeginHorizontalToolbar(HeaderStyle);
            {
                var icon = _expanded ? OpenIcon : ClosedIcon;

                int iconSize = (int) HeaderStyle.fixedHeight;
                if (CustomEditorGUI.IconButton(icon, iconSize, iconSize))
                    _expanded = !_expanded;

                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                using (new eUtility.DisabledGroup(true))
                {
                    if (Target is Object unityTarget)
                        EditorGUILayout.ObjectField(GUIContent.none, unityTarget, unityTarget.GetType(), true);
                    else
                        EditorGUILayout.LabelField(Target.GetType().Name, CustomGUIStyles.SubtitleCentered);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            CustomEditorGUI.EndHorizontalToolbar();
        }

        public void DrawEditor()
        {
            if (Target == null)
                return;
            
#if ODIN_INSPECTOR
            if (_tree == null) 
                _tree = PropertyTree.Create(Target);

            if (SirenixEditorGUI.BeginFadeGroup(Target, _expanded))
                _tree.Draw(true);
        
            SirenixEditorGUI.EndFadeGroup();
#else
            if (_view == null)
            {
                _view = new DrawablePropertyView(new RootHostInfo(this));
                _view.RepaintRequested += RequestRepaint;
            }
            
            if (_expanded)
                _view.DrawLayout();
#endif
        }

        public bool CanDraw() => Target != null;
        
        public void Draw()
        {
            DrawHeader();
            DrawEditor();
        }

        public bool HasPreviewGUI() => false;
        public void DrawPreview(Rect rect) {}

        public void Destroy()
        {
            // Nothing to do
        }

#if ODIN_VALIDATOR
    #if ODIN_INSPECTOR_3 && !OLD_ODIN_VALIDATION_ENGINE

        public void Validate(Object root)
        {
            var list = new List<PersistentValidationResultBatch>();
            Validate(root, ref list);
        }
        
        public void Validate(Object root, ref List<PersistentValidationResultBatch> list)
        {
            var runner = new OdinValidationRunner();
            // TODO root or Target??
            var resultBatches = runner.ValidateUnityObjectRecursively(root).ToArray();
            list.AddRange(resultBatches);
        }
    #else
    
        public void Validate(Object root)
        {
            var list = new List<ValidationResult>();
            Validate(root, ref list);
        }
    
        public void Validate(Object root, ref List<ValidationResult> list)
        {
        #if ODIN_INSPECTOR_3
           var runner = new ValidationRunner();
            // TODO root or Target??
            runner.ValidateUnityObjectRecursively(root, ref list);
        #else
            var runner = new ValidationRunner();
            var selector = new DefaultValidationMemberSelector();
            runner.ValidateMembers(Target, selector, root, false, ref list);
        #endif
        }
    #endif
#endif
        public void RequestRepaint()
        {
            if (_repaintHandler != null)
                _repaintHandler.RequestRepaint();
        }

        public void UpdateRequestTarget(IRepaintable target)
        {
            _repaintHandler = target;
        }
    }
}