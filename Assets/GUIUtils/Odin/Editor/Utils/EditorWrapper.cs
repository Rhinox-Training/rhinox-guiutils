using Rhinox.GUIUtils.Editor;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities.Editor;
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

        private PropertyTree _tree;
        private bool _expanded;

        private static GUIStyle _headerStyle;
        public static GUIStyle HeaderStyle => _headerStyle ?? (_headerStyle = new GUIStyle(SirenixGUIStyles.ToggleGroupTitleBg)
        {
            fixedHeight = 26
        });

        public EditorWrapper(object target, bool expanded = true)
        {
            Target = target;
            _expanded = expanded;
            _tree = null;
        }

        public void DrawHeader()
        {
            if (Target == null)
                return;
            
            SirenixEditorGUI.BeginIndentedHorizontal(HeaderStyle);
            if (SirenixEditorGUI.IconButton(_expanded ? EditorIcons.TriangleDown : EditorIcons.TriangleRight, 20, 20))
                _expanded = !_expanded;

            using (new eUtility.DisabledGroup(true))
            {
                if (Target is Object unityTarget)
                    SirenixEditorFields.UnityObjectField(GUIContent.none, unityTarget, unityTarget.GetType(), true);
                else
                    SirenixEditorGUI.Title(Target.GetType().Name, null, TextAlignment.Left, true);
            }
            
            SirenixEditorGUI.EndIndentedHorizontal();
        }

        public void DrawEditor()
        {
            if (Target == null)
                return;
            
            if (_tree == null) _tree = PropertyTree.Create(Target);

            if (SirenixEditorGUI.BeginFadeGroup(Target, _expanded))
                _tree.Draw(true);
        
            SirenixEditorGUI.EndFadeGroup();
        }

        public void Draw()
        {
            DrawHeader();
            DrawEditor();
        }

#if ODIN_VALIDATOR
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
    }
}