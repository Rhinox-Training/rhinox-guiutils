#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public static class CustomGUIStyles
    {
        private static GUIStyle _titleStyle;
        public static GUIStyle Title
        {
            get
            {
                if (_titleStyle == null)
#if UNITY_EDITOR
                    _titleStyle = new GUIStyle(EditorStyles.label);
#else
                    _titleStyle = new GUIStyle("label");
#endif
                return _titleStyle;
            }
        }
        
        private static GUIStyle _boldTitleStyle;
        public static GUIStyle BoldTitle
        {
            get
            {
                if (_boldTitleStyle == null)
                    _boldTitleStyle = new GUIStyle(CustomGUIStyles.Title)
                    {
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                return _boldTitleStyle;
            }
        }

        private static GUIStyle _subtitleStyle;
        public static GUIStyle Subtitle
        {
            get
            {
                if (_subtitleStyle == null)
                {
                    _subtitleStyle = new GUIStyle(CustomGUIStyles.Title)
                    {
                        font = GUI.skin.button.font,
                        fontSize = 10,
                        contentOffset = new Vector2(0.0f, -3f),
                        fixedHeight = 16f
                    };
                    Color textColor = _subtitleStyle.normal.textColor;
                    textColor.a *= 0.7f;
                    _subtitleStyle.normal.textColor = textColor;
                }
                return _subtitleStyle;
            }
        }
        
        private static GUIStyle _subtitleStyleCentered;
        public static GUIStyle SubtitleCentered
        {
            get
            {
                if (_subtitleStyleCentered == null)
                    _subtitleStyleCentered = new GUIStyle(CustomGUIStyles.Subtitle)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                return _subtitleStyleCentered;
            }
        }

        private static GUIStyle _subtitleStyleRightAligned;
        public static GUIStyle SubtitleRight
        {
            get
            {
                if (_subtitleStyleCentered == null)
                    _subtitleStyleCentered = new GUIStyle(CustomGUIStyles.Subtitle)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                return _subtitleStyleRightAligned;
            }
        }
        
        private static GUIStyle _labelStyle;
        public static GUIStyle Label
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle(
#if UNITY_EDITOR
                        EditorStyles.label
#else
                        "label"
#endif
                    );
                }

                return _labelStyle;
            }
        }
        
        private static GUIStyle _labelStyleBold;
        public static GUIStyle BoldLabel
        {
            get
            {
                if (_labelStyleBold == null)
                {
                    _labelStyleBold = new GUIStyle(
#if UNITY_EDITOR
                        EditorStyles.label
#else
                        "label"
#endif
                    )
                    {
                        fontStyle = FontStyle.Bold,
                    };
                }

                return _labelStyleBold;
            }
        }
        
        private static GUIStyle _labelStyleBoldCentered;
        public static GUIStyle BoldLabelCentered
        {
            get
            {
                if (_labelStyleBoldCentered == null)
                {
                    _labelStyleBoldCentered = new GUIStyle(
#if UNITY_EDITOR
                        EditorStyles.label
#else
                        "label"
#endif
                    )
                    {
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return _labelStyleBoldCentered;
            }
        }

        private static GUIStyle _labelStyleUnpadded;
        public static GUIStyle UnpaddedLabel
        {
            get
            {
                if (_labelStyleUnpadded == null)
                {
                    _labelStyleUnpadded = new GUIStyle(
                        #if UNITY_EDITOR
                        EditorStyles.label
                        #else
                        "label"
                        #endif
                        )
                    {
                        padding = new RectOffset(),
                        fontStyle = FontStyle.Normal,
                    };
                    //_labelStyleUnpadded.font.material.color = Color.white;
                }

                return _labelStyleUnpadded;
            }
        }
        
        private static GUIStyle _toolbarBackground;
        public static GUIStyle ToolbarBackground
        {
            get
            {
                if (_toolbarBackground == null)
                {
                    _toolbarBackground = new GUIStyle("Box")
                    {
                        padding = new RectOffset(0, 1, 0, 0),
                        stretchHeight = true,
                        stretchWidth = true,
                        fixedHeight = 0.0f
                    };
                    
#if UNITY_2019_3_OR_NEWER
                    _toolbarBackground.padding = new RectOffset(0, 0, 0, 0);
#endif
                }
                return _toolbarBackground;
            }
        }

        private static GUIStyle _toolbarTab;
        public static GUIStyle ToolbarTab
        {
            get
            {
                if (_toolbarTab == null)
                    _toolbarTab = new GUIStyle(
                        #if UNITY_EDITOR
                            EditorStyles.toolbarButton
                        #else
                            "Button"
                        #endif
                        )
                    {
                        fixedHeight = 0.0f,
                        stretchHeight = true,
                        stretchWidth = true
                    };
                return _toolbarTab;
            }
        }
    }
}