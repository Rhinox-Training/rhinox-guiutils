#if UNITY_EDITOR
using UnityEditor;
#endif
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public static class CustomGUIStyles
    {
        private static bool IsDarkMode()
        {
#if UNITY_EDITOR
            return EditorGUIUtility.isProSkin;
#else
            return true;
#endif
        }
        
        // Colors
        public static readonly Color BorderColor = IsDarkMode() ? new Color(0.11f, 0.11f, 0.11f, 0.8f) : new Color(0.38f, 0.38f, 0.38f, 0.6f);
        public static readonly Color LightBorderColor = new Color(.353f, .353f,  .353f);
        public static readonly Color BoxBackgroundColor = IsDarkMode() ? new Color(.22f, .22f, .22f) : new Color(.76f, .76f, .76f);
        public static readonly Color DarkEditorBackground = IsDarkMode() ? new Color(0.192f, 0.192f, 0.192f) : new Color(0.0f, 0.0f, 0.0f, 0.0f);
        public static readonly Color HoverColor = IsDarkMode() ? new Color(.27f, .27f, .27f) : new Color(.7f, .7f, .7f);
        public static readonly Color SelectedColor = IsDarkMode() ? new Color(.17f, .36f, .53f) : new Color(.23f, .45f, .69f);
        public static readonly Color UnfocusedSelectedColor = IsDarkMode() ? new Color(.3f, .3f, .3f) : new Color(.68f, .68f, .68f);

        
        private static GUIStyle _boxStyle;
        public static GUIStyle Box
        {
            get
            {
                if (_boxStyle == null)
                    _boxStyle = new GUIStyle("box")
                    {
                        margin = new RectOffset(),
                        normal = new GUIStyleState()
                        {
                            background = Utility.GetColorTexture(BoxBackgroundColor)
                        }
                    };
                return _boxStyle;
            }
        }
        
        private static GUIStyle _cardStyle;
        public static GUIStyle Card
        {
            get
            {
                if (_cardStyle == null)
                    _cardStyle = new GUIStyle("sv_iconselector_labelselection")
                    {
                        padding = new RectOffset(15, 15, 15, 15),
                        margin = new RectOffset(0, 0, 0, 0),
                        stretchHeight = false
                    };;
                return _cardStyle;
            }
        }
        
#region Title Styles
        private static GUIStyle _titleStyle;
        public static GUIStyle Title
        {
            get
            {
                if (_titleStyle == null)
                    _titleStyle = new GUIStyle("ControlLabel");
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
                        fontStyle = FontStyle.Bold
                    };
                return _boldTitleStyle;
            }
        }
        
        private static GUIStyle _titleStyleCentered;
        public static GUIStyle TitleCentered
        {
            get
            {
                if (_titleStyleCentered == null)
                    _titleStyleCentered = new GUIStyle(Title)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                return _titleStyleCentered;
            }
        }
        
        private static GUIStyle _boldTitleStyleCentered;
        public static GUIStyle BoldTitleCentered
        {
            get
            {
                if (_boldTitleStyleCentered == null)
                    _boldTitleStyleCentered = new GUIStyle(CustomGUIStyles.TitleCentered)
                    {
                        fontStyle = FontStyle.Bold
                    };
                return _boldTitleStyleCentered;
            }
        }
        
        private static GUIStyle _titleStyleRight;
        
        public static GUIStyle TitleRight
        {
            get
            {
                if (_titleStyleRight == null)
                    _titleStyleRight = new GUIStyle(Title)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                return _titleStyleRight;
            }
        }
        
        private static GUIStyle _boldTitleStyleRight;
        public static GUIStyle BoldTitleRight
        {
            get
            {
                if (_boldTitleStyleRight == null)
                    _boldTitleStyleRight = new GUIStyle(CustomGUIStyles.TitleRight)
                    {
                        fontStyle = FontStyle.Bold
                    };
                return _boldTitleStyleRight;
            }
        }
        
        private static GUIStyle _titleBackgroundStyle;
        public static GUIStyle TitleBackground
        {
            get
            {
                if (_titleBackgroundStyle == null)
                    _titleBackgroundStyle = new GUIStyle("ShurikenModuleTitle")
                    {
                        padding = new RectOffset(0, 1, 0, 0),
                        fontStyle = FontStyle.Bold,
                        fontSize = 12,
                        alignment = TextAnchor.MiddleLeft,
                        stretchHeight = false,
                        stretchWidth = true,
                        fixedHeight = 24.0f
                    };
                return _titleBackgroundStyle;
            }
        }
        
        
        private static GUIStyle _titleBackgroundStyleCentered;
        public static GUIStyle TitleBackgroundCentered
        {
            get
            {
                if (_titleBackgroundStyleCentered == null)
                    _titleBackgroundStyleCentered = new GUIStyle(TitleBackground)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                return _titleBackgroundStyleCentered;
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

        private static GUIStyle _subtitleStyleRight;
        public static GUIStyle SubtitleRight
        {
            get
            {
                if (_subtitleStyleRight == null)
                    _subtitleStyleRight = new GUIStyle(CustomGUIStyles.Subtitle)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                return _subtitleStyleRight;
            }
        }
#endregion // Title Styles

#region Label Styles

        private static GUIStyle _labelStyle;
        public static GUIStyle Label
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle("ControlLabel");
                }

                return _labelStyle;
            }
        }
        
        private static GUIStyle _labelStyleCentered;
        public static GUIStyle CenteredLabel
        {
            get
            {
                if (_labelStyleCentered == null)
                {
                    _labelStyleCentered = new GUIStyle(CustomGUIStyles.Label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return _labelStyleCentered;
            }
        }
        
        private static GUIStyle _labelStyleRight;
        public static GUIStyle LabelRight
        {
            get
            {
                if (_labelStyleRight == null)
                {
                    _labelStyleRight = new GUIStyle(CustomGUIStyles.Label)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }

                return _labelStyleRight;
            }
        }
        
        private static GUIStyle _labelStyleCenteredWithHover;
        public static GUIStyle CenteredLabelWithHover
        {
            get
            {
                if (_labelStyleCenteredWithHover == null)
                {
                    _labelStyleCenteredWithHover = new GUIStyle(CustomGUIStyles.Label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                    _labelStyleCenteredWithHover.hover = new GUIStyleState() {
                        textColor = Color.white
                    };
                }

                return _labelStyleCenteredWithHover;
            }
        }
        
        private static GUIStyle _labelStyleBold;
        public static GUIStyle BoldLabel
        {
            get
            {
                if (_labelStyleBold == null)
                {
                    _labelStyleBold = new GUIStyle(CustomGUIStyles.Label)
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
                    _labelStyleBoldCentered = new GUIStyle(CustomGUIStyles.Label)
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
                    _labelStyleUnpadded = new GUIStyle(CustomGUIStyles.Label)
                    {
                        padding = new RectOffset(),
                        fontStyle = FontStyle.Normal,
                    };
                    //_labelStyleUnpadded.font.material.color = Color.white;
                }

                return _labelStyleUnpadded;
            }
        }
        
        private static GUIStyle _miniLabelStyleLeft;
        public static GUIStyle MiniLabel
        {
            get
            {
                if (_miniLabelStyleLeft == null)
                {
                    _miniLabelStyleLeft = new GUIStyle("MiniLabel")
                    {
                        alignment = TextAnchor.MiddleLeft,
                        clipping = TextClipping.Clip,
                        margin = new RectOffset(4, 4, 4, 4),
                        normal = {
                            textColor = Color.grey
                        }
                    };
                }
                return _miniLabelStyleLeft;
            }
        }

        private static GUIStyle _miniLabelStyleCentered;
        public static GUIStyle MiniLabelCentered
        {
            get
            {
                if (_miniLabelStyleCentered == null)
                {
                    _miniLabelStyleCentered = new GUIStyle(CustomGUIStyles.MiniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return _miniLabelStyleCentered;
            }
        }

        private static GUIStyle _miniLabelStyleRight;
        public static GUIStyle MiniLabelRight
        {
            get
            {
                if (_miniLabelStyleRight == null)
                {
                    _miniLabelStyleRight = new GUIStyle(CustomGUIStyles.MiniLabel)
                    {
                        alignment = TextAnchor.MiddleRight
                    };
                }
                return _miniLabelStyleRight;
            }
        }
#endregion //Label Styles

#region Clean Styles
        // =============================================================================================================
        // Clean styles
        private static GUIStyle _cleanStyle;
        public static GUIStyle Clean
        {
            get
            {
                if (_cleanStyle == null)
                {
                    _cleanStyle = new GUIStyle()
                    {
                        overflow = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _cleanStyle;
            }
        }
        
        private static GUIStyle _cleanStyleTextField;
        public static GUIStyle CleanTextField
        {
            get
            {
                if (_cleanStyleTextField == null)
                {
                    _cleanStyleTextField = new GUIStyle("TextField")
                    {
                        overflow = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                    };
                }

                return _cleanStyleTextField;
            }
        }
        private static GUIStyle _cleanLabelField;
        public static GUIStyle CleanLabelField
        {
            get
            {
                if (_cleanLabelField == null)
                {
                    _cleanLabelField = new GUIStyle("ControlLabel")
                    {
                        overflow = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                    };
                }

                return _cleanLabelField;
            }
        }
#endregion
 
#region Toolbar Styles

        // =============================================================================================================
        // Toolbar styles
        
        private static GUIStyle _toolbarBackground;
        public static GUIStyle ToolbarBackground
        {
            get
            {
                if (_toolbarBackground == null)
                {
                    _toolbarBackground = new GUIStyle("toolbar")
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                        stretchHeight = true,
                        stretchWidth = true,
                        fixedHeight = 0.0f,
                    };
                }
                return _toolbarBackground;
            }
        }
        
        private static GUIStyle _toggleGroupBackground;
        public static GUIStyle ToggleGroupBackground
        {
            get
            {
                if (_toggleGroupBackground == null)
                {
                    _toggleGroupBackground = new GUIStyle("HelpBox")
                    {
                        overflow = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _toggleGroupBackground;
            }
        }

        private static GUIStyle _toolbarTab;
        public static GUIStyle ToolbarTab
        {
            get
            {
                if (_toolbarTab == null)
                    _toolbarTab = new GUIStyle("toolbarbutton")
                    {
                        fixedHeight = 0.0f,
                        stretchHeight = true,
                        stretchWidth = true
                    };
                return _toolbarTab;
            }
        }
        
        private static GUIStyle _toolbarCentered;
        public static GUIStyle ToolbarButtonCentered
        {
            get
            {
                if (_toolbarCentered == null)
                    _toolbarCentered = new GUIStyle("toolbarbutton")
                    {
                        fixedHeight = 0.0f,
                        alignment = TextAnchor.MiddleCenter,
                        stretchHeight = true,
                        stretchWidth = false
                    };
                return _toolbarCentered;
            }
        }
        
        private static GUIStyle _toolbarSearchTextField;
        public static GUIStyle ToolbarSearchTextField
        {
            get
            {
                if (_toolbarSearchTextField == null)
                    _toolbarSearchTextField = new GUIStyle("ToolbarSeachTextField");
                return _toolbarSearchTextField;
            }
        }
        
        private static GUIStyle _toolbarSearchCancelButton;
        public static GUIStyle ToolbarSearchCancelButton
        {
            get
            {
                if (_toolbarSearchCancelButton == null)
                    _toolbarSearchCancelButton = new GUIStyle("ToolbarSeachCancelButton");
                return _toolbarSearchCancelButton;
            }
        }
#endregion

#region Button Styles
        // =============================================================================================================
        // Button Styles

        private static GUIStyle _iconButtonStyle;
        public static GUIStyle IconButton
        {
            get
            {
                if (_iconButtonStyle == null)
                {
                    _iconButtonStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(1, 1, 1, 1)
                    };
                }
                    
                return _iconButtonStyle;
            }
        }

        private static GUIStyle _buttonStyle;
        public static GUIStyle Button
        {
            get
            {
                if (_buttonStyle == null)
                    _buttonStyle = new GUIStyle((GUIStyle)nameof(Button));
                return _buttonStyle;
            }
        }
        
        private static GUIStyle _buttonStyleSelected;
        public static GUIStyle ButtonSelected
        {
            get
            {
                if (_buttonStyleSelected == null)
                    _buttonStyleSelected = new GUIStyle(CustomGUIStyles.Button)
                    {
                        normal = new GUIStyle(CustomGUIStyles.Button).onNormal
                    };
                return _buttonStyleSelected;
            }
        }

        private static GUIStyle _buttonStyleLeft;
        public static GUIStyle ButtonLeft
        {
            get
            {
                if (_buttonStyleLeft == null)
                {
                    if (GUI.skin.FindStyle(nameof(ButtonLeft)) != null)
                        _buttonStyleLeft = new GUIStyle((GUIStyle) nameof(ButtonLeft));
                    else
                        _buttonStyleLeft = new GUIStyle((GUIStyle) "button");
                }

                return _buttonStyleLeft;
            }
        }
        
        private static GUIStyle _iconButtonStyleLeft;
        public static GUIStyle IconButtonLeft
        {
            get
            {
                if (_iconButtonStyleLeft == null)
                {
                    _iconButtonStyleLeft = new GUIStyle(CustomGUIStyles.ButtonLeft)
                    {
                        padding = new RectOffset(1, 1, 1, 1)
                    };
                }

                return _iconButtonStyleLeft;
            }
        }
        
        private static GUIStyle _buttonStyleLeftSelected;
        public static GUIStyle ButtonLeftSelected
        {
            get
            {
                if (_buttonStyleLeftSelected == null)
                    _buttonStyleLeftSelected = new GUIStyle(CustomGUIStyles.ButtonLeft)
                    {
                        normal = CustomGUIStyles.ButtonLeft.onNormal
                    };
                return _buttonStyleLeftSelected;
            }
        }

        private static GUIStyle _buttonStyleMiddle;
        public static GUIStyle ButtonMid
        {
            get
            {
                if (_buttonStyleMiddle == null)
                {
                    if (GUI.skin.FindStyle(nameof(ButtonMid)) != null)
                        _buttonStyleMiddle = new GUIStyle((GUIStyle)nameof(ButtonMid));
                    else
                        _buttonStyleMiddle = new GUIStyle((GUIStyle) "button");
                }
                return _buttonStyleMiddle;
            }
        }
        
        private static GUIStyle _buttonStyleMidSelected;
        public static GUIStyle ButtonMidSelected
        {
            get
            {
                if (_buttonStyleMidSelected == null)
                    _buttonStyleMidSelected = new GUIStyle(CustomGUIStyles.ButtonMid)
                    {
                        normal = CustomGUIStyles.ButtonMid.onNormal
                    };
                return _buttonStyleMidSelected;
            }
        }
        
        private static GUIStyle _iconButtonStyleMid;
        public static GUIStyle IconButtonMid
        {
            get
            {
                if (_iconButtonStyleMid == null)
                {
                    _iconButtonStyleMid = new GUIStyle(CustomGUIStyles.ButtonMid)
                    {
                        padding = new RectOffset(1, 1, 1, 1)
                    };
                }

                return _iconButtonStyleMid;
            }
        }

        private static GUIStyle _buttonStyleRight;
        public static GUIStyle ButtonRight
        {
            get
            {
                if (_buttonStyleRight == null)
                {
                    if (GUI.skin.FindStyle(nameof(ButtonRight)) != null)
                        _buttonStyleRight = new GUIStyle((GUIStyle)nameof(ButtonRight));
                    else
                        _buttonStyleRight = new GUIStyle((GUIStyle) "button");
                }
                return _buttonStyleRight;
            }
        }
        
        private static GUIStyle _buttonStyleRightSelected;
        public static GUIStyle ButtonRightSelected
        {
            get
            {
                if (_buttonStyleRightSelected == null)
                    _buttonStyleRightSelected = new GUIStyle(CustomGUIStyles.ButtonRight)
                    {
                        normal = CustomGUIStyles.ButtonRight.onNormal
                    };
                return _buttonStyleRightSelected;
            }
        }
        
        private static GUIStyle _iconButtonStyleRight;
        public static GUIStyle IconButtonRight
        {
            get
            {
                if (_iconButtonStyleRight == null)
                {
                    _iconButtonStyleRight = new GUIStyle(CustomGUIStyles.ButtonRight)
                    {
                        padding = new RectOffset(1, 1, 1, 1)
                    };
                }

                return _iconButtonStyleRight;
            }
        }
        
        public static GUIStyle GetButtonGroupStyle(int buttonI, int maxI, bool selected = false)
        {
            if (buttonI == 0 && maxI <= 1)
                return selected ? CustomGUIStyles.ButtonSelected : CustomGUIStyles.Button;
            if (buttonI == 0) return selected ? CustomGUIStyles.ButtonLeftSelected : CustomGUIStyles.ButtonLeft;
            if (buttonI >= maxI - 1) return selected ? CustomGUIStyles.ButtonRightSelected : CustomGUIStyles.ButtonRight;
            return selected ? CustomGUIStyles.ButtonMidSelected : CustomGUIStyles.ButtonMid;
        }

        private static GUIStyle _miniButtonStyle;
        public static GUIStyle MiniButton
        {
            get
            {
                if (_miniButtonStyle == null)
                    _miniButtonStyle = new GUIStyle("miniButton");
                return _miniButtonStyle;
            }
        }

        private static GUIStyle _miniButtonStyleSelected;
        public static GUIStyle MiniButtonSelected
        {
            get
            {
                if (_miniButtonStyleSelected == null)
                    _miniButtonStyleSelected = new GUIStyle(CustomGUIStyles.MiniButton)
                    {
                        normal = new GUIStyle(CustomGUIStyles.MiniButton).onNormal
                    };
                return _miniButtonStyleSelected;
            }
        }

        private static GUIStyle _miniButtonStyleLeft;
        public static GUIStyle MiniButtonLeft
        {
            get
            {
                if (_miniButtonStyleLeft == null)
                    _miniButtonStyleLeft = new GUIStyle("miniButtonLeft");
                return _miniButtonStyleLeft;
            }
        }

        private static GUIStyle _miniButtonStyleLeftSelected;
        public static GUIStyle MiniButtonLeftSelected
        {
            get
            {
                if (_miniButtonStyleLeftSelected == null)
                    _miniButtonStyleLeftSelected = new GUIStyle(CustomGUIStyles.MiniButtonLeft)
                    {
                        normal = new GUIStyle(CustomGUIStyles.MiniButtonLeft).onNormal
                    };
                return _miniButtonStyleLeftSelected;
            }
        }

        private static GUIStyle _miniButtonStyleMid;
        public static GUIStyle MiniButtonMid
        {
            get
            {
                if (_miniButtonStyleMid == null)
                    _miniButtonStyleMid = new GUIStyle("miniButtonMid");
                return _miniButtonStyleMid;
            }
        }

        private static GUIStyle _miniButtonStyleMidSelected;
        public static GUIStyle MiniButtonMidSelected
        {
            get
            {
                if (_miniButtonStyleMidSelected == null)
                    _miniButtonStyleMidSelected = new GUIStyle(CustomGUIStyles.MiniButtonMid)
                    {
                        normal = new GUIStyle(CustomGUIStyles.MiniButtonMid).onNormal
                    };
                return _miniButtonStyleMidSelected;
            }
        }

        private static GUIStyle _miniButtonStyleRight;
        public static GUIStyle MiniButtonRight
        {
            get
            {
                if (_miniButtonStyleRight == null)
                    _miniButtonStyleRight = new GUIStyle("miniButtonRight");
                return _miniButtonStyleRight;
            }
        }

        private static GUIStyle _miniButtonStyleRightSelected;
        public static GUIStyle MiniButtonRightSelected
        {
            get
            {
                if (_miniButtonStyleRightSelected == null)
                    _miniButtonStyleRightSelected = new GUIStyle(CustomGUIStyles.MiniButtonRight)
                    {
                        normal = new GUIStyle(CustomGUIStyles.MiniButtonRight).onNormal
                    };
                return _miniButtonStyleRightSelected;
            }
        }
        
        public static GUIStyle GetMiniButtonGroupStyle(int buttonI, int maxI, bool selected = false)
        {
            if (buttonI == 0 && maxI <= 1)
                return selected ? CustomGUIStyles.MiniButtonSelected : CustomGUIStyles.MiniButton;
            if (buttonI == 0) return selected ? CustomGUIStyles.MiniButtonLeftSelected : CustomGUIStyles.MiniButtonLeft;
            if (buttonI >= maxI - 1) return selected ? CustomGUIStyles.MiniButtonRightSelected : CustomGUIStyles.MiniButtonRight;
            return selected ? CustomGUIStyles.MiniButtonMidSelected : CustomGUIStyles.MiniButtonMid;
        }
#endregion
    }
}