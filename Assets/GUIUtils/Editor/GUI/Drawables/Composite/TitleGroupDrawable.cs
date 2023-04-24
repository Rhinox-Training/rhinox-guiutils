using System;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TitleGroupDrawable : BaseVerticalGroupDrawable<TitleGroupAttribute>
    {
        public override float ElementHeight
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Title))
                    return base.ElementHeight;
                
                var titleHeight = EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding;
                
                // TODO subtitle

                if (HorizontalLine)
                    titleHeight += 2 + CustomGUIUtility.Padding;
                
                return base.ElementHeight + titleHeight;
            }
        }

        public string Title { get; set; }
        public string Subtitle { get; set; }

        public TitleAlignments Alignment = TitleAlignments.Left;
        public bool Bold = true;
        public bool HorizontalLine = true;

        public GUIStyle TitleStyle { get; set; }
        public GUIStyle SubtitleStyle { get; set; }


        public TitleGroupDrawable(GroupedDrawable parent, string groupID, int order) : base(parent, groupID, order)
        {
        }

        public override void Draw(GUIContent label)
        {
            var rect = EditorGUILayout.BeginVertical(CustomGUIStyles.Clean, GetLayoutOptions(_size));

            if (!string.IsNullOrWhiteSpace(Title))
            {
                GUILayout.Label(GUIContentHelper.TempContent(Title), TitleStyle);
                
                // TODO subtitle

                if (HorizontalLine)
                {
                    CustomEditorGUI.HorizontalLine(CustomGUIStyles.LightBorderColor, thickness: 1);
                    GUILayout.Space(1.0f + CustomGUIUtility.Padding);
                }
            }
            
            base.Draw(label);
            
            EditorGUILayout.EndVertical();
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            if (!string.IsNullOrWhiteSpace(Title))
            {
                var labelRect = rect.AlignTop(EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, GUIContentHelper.TempContent(Title), TitleStyle);
                rect.yMin += labelRect.height + CustomGUIUtility.Padding;
                
                // TODO subtitle

                if (HorizontalLine)
                {
                    CustomEditorGUI.HorizontalLine(rect, CustomGUIStyles.LightBorderColor);
                    rect.yMin += 2 + CustomGUIUtility.Padding;
                }
            }
            
            base.Draw(rect, label);
        }

        protected override void ParseAttributeSmart(IOrderedDrawable child, TitleGroupAttribute attr)
        {
        }

        protected override void ParseAttributeSmart(TitleGroupAttribute attr)
        {
            // TODO: current we just take the last entry, different entries having different data is invalid?
                
            if (!attr.GroupName.IsNullOrEmpty())
                Title = attr.GroupName;
                
            if (attr.Order != 0)
                SetOrder(attr.Order);
                
            if (!attr.HorizontalLine) // default is true so only set if you specify it
                HorizontalLine = false;
                
            if (attr.Alignment != TitleAlignments.Left)
                Alignment = attr.Alignment;
            if (!attr.BoldTitle) // default is true so only set if you specify it
                Bold = false;
                
            TitleStyle = UpdateStyle();
            _parent?.EnsureSizeFits(_size);
        }

        private GUIStyle UpdateStyle()
        {
            switch (Alignment)
            {
                case TitleAlignments.Left:
                    SubtitleStyle = CustomGUIStyles.Subtitle;
                    return Bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title;
                
                case TitleAlignments.Centered:
                    SubtitleStyle = CustomGUIStyles.SubtitleCentered;
                    return Bold ? CustomGUIStyles.BoldTitleCentered : CustomGUIStyles.TitleCentered;
                
                case TitleAlignments.Right:
                    SubtitleStyle = CustomGUIStyles.SubtitleRight;
                    return Bold ? CustomGUIStyles.BoldTitleRight : CustomGUIStyles.TitleRight;
                
                case TitleAlignments.Split:
                    SubtitleStyle = CustomGUIStyles.SubtitleRight;
                    return Bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title;
                
                default:
                    return null;
            }
        }
    }
}