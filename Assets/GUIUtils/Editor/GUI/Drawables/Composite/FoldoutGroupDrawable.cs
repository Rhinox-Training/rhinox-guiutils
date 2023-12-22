using Sirenix.OdinInspector;
using UnityEngine;
using Rhinox.Lightspeed;
using UnityEditor;


namespace Rhinox.GUIUtils.Editor
{
    public class FoldoutGroupDrawable : BaseVerticalGroupDrawable<FoldoutGroupAttribute>
    {
        public string Title { get; set; }

        public bool Foldout = true;

        public FoldoutGroupDrawable(GroupedDrawable parent, string groupID, float order) : base(parent, groupID, order)
        {
        }

        public override void Draw(GUIContent label)
        {
            if (!string.IsNullOrWhiteSpace(Title))
            {
                Foldout = eUtility.Foldout(Foldout, Title);
                if (Foldout)
                {
                    using (new eUtility.IndentedLayout())
                    {
                        base.Draw(label);
                    }
                }
            }
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            if (!string.IsNullOrWhiteSpace(Title))
            {
                var labelRect = rect.AlignTop(EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, GUIContentHelper.TempContent(Title));
                rect.yMin += labelRect.height + CustomGUIUtility.Padding;
            }

            base.Draw(rect, label);
        }

        protected override void ParseAttributeSmart(IOrderedDrawable child, FoldoutGroupAttribute attr)
        {
        }

        protected override void ParseAttributeSmart(FoldoutGroupAttribute attr)
        {
            if (!attr.GroupName.IsNullOrEmpty())
                Title = attr.GroupName;

            if (attr.Order != 0)
                SetOrder(attr.Order);

            _parent?.EnsureSizeFits(_size);
        }
    }
}