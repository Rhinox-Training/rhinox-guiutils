using Rhinox.GUIUtils.Editor;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR_3 && ODIN_VALIDATOR
using Sirenix.OdinValidator.Editor;
#endif

namespace Rhinox.GUIUtils.Odin.Editor
{

    /// <summary>
    /// Wrapper for an object to draw a certain object keeping editing capabilities
    /// </summary>
    public struct EditorWrapper
    {
        [ShowIf(nameof(_expanded)), ShowInInspector]
        [InlineEditor(Expanded = true, ObjectFieldMode = InlineEditorObjectFieldModes.CompletelyHidden)]
        public object Target;
        
#if ODIN_INSPECTOR
        private PropertyTree _tree;
#endif
        private bool _expanded;

        private static GUIStyle _headerStyle;
        public static GUIStyle HeaderStyle => _headerStyle ?? (_headerStyle = new GUIStyle(CustomGUIStyles.ToggleGroupBackground)
        {
            fixedHeight = 26
        });

        public EditorWrapper(object target, bool expanded = true)
        {
            Target = target;
            _expanded = expanded;
#if ODIN_INSPECTOR
            _tree = null;
#endif
        }

        public void DrawHeader()
        {
            if (Target == null)
                return;
            
            CustomEditorGUI.BeginHorizontalToolbar(HeaderStyle);
            if (CustomEditorGUI.IconButton(_expanded ? UnityIcon.AssetIcon("Fa_ArrowDown") : UnityIcon.AssetIcon("Fa_ArrowRight") , 20, 20))
                _expanded = !_expanded;

            using (new eUtility.DisabledGroup(true))
            {
                if (Target is Object unityTarget)
                    EditorGUILayout.ObjectField(GUIContent.none, unityTarget, unityTarget.GetType(), true);
                else
                    EditorGUILayout.LabelField(Target.GetType().Name, CustomGUIStyles.SubtitleCentered);
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
#endif
        }

        public void Draw()
        {
            DrawHeader();
            DrawEditor();
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
    }
}