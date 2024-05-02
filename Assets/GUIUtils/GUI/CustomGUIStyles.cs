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
        public static GUIStyle Box =>
            _boxStyle ?? (_boxStyle = new GUIStyle("box")
            {
                margin = new RectOffset(),
                normal = new GUIStyleState()
                {
                    background = Utility.GetColorTexture(BoxBackgroundColor)
                }
            });
        
        private static GUIStyle _foldoutBoxStyle;
        public static GUIStyle FoldoutBoxStyle =>
            _foldoutBoxStyle ?? (_foldoutBoxStyle = new GUIStyle("box")
            {
                margin = new RectOffset(),
                padding = new RectOffset(),
                normal = new GUIStyleState()
                {
                    background = Utility.GetColorTexture(BoxBackgroundColor)
                }
            });

        private static GUIStyle _cardStyle;
        public static GUIStyle Card =>
            _cardStyle ?? (_cardStyle = new GUIStyle("sv_iconselector_labelselection")
            {
                padding = new RectOffset(15, 15, 15, 15),
                margin = new RectOffset(0, 0, 0, 0),
                stretchHeight = false
            });
        
        private static GUIStyle _boldFoldoutStyle;
        public static GUIStyle BoldFoldout =>
            _boldFoldoutStyle ?? (_boldFoldoutStyle = new GUIStyle("foldout")
            {
                fontStyle = FontStyle.Bold
            });
        
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
        public static GUIStyle Label =>
            _labelStyle ?? (_labelStyle = new GUIStyle("ControlLabel")
            {
                alignment = TextAnchor.MiddleLeft
            });
        

        private static GUIStyle _labelStyleCentered;
        public static GUIStyle CenteredLabel =>
            _labelStyleCentered ?? (_labelStyleCentered = new GUIStyle(CustomGUIStyles.Label)
            {
                alignment = TextAnchor.MiddleCenter
            });

        
        private static GUIStyle _labelStyleRight;
        public static GUIStyle LabelRight =>
            _labelStyleRight ?? (_labelStyleRight = new GUIStyle(CustomGUIStyles.Label)
            {
                alignment = TextAnchor.MiddleRight
            });

        
        private static GUIStyle _labelStyleCenteredWithHover;
        public static GUIStyle CenteredLabelWithHover =>
            _labelStyleCenteredWithHover ?? (_labelStyleCenteredWithHover =
                new GUIStyle(CustomGUIStyles.Label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    hover = new GUIStyleState()
                    {
                        textColor = Color.white
                    }
                });

        
        private static GUIStyle _labelStyleBold;
        public static GUIStyle BoldLabel =>
            _labelStyleBold ?? (_labelStyleBold = new GUIStyle(CustomGUIStyles.Label)
            {
                fontStyle = FontStyle.Bold,
            });

        
        private static GUIStyle _labelStyleBoldCentered;
        public static GUIStyle BoldLabelCentered =>
            _labelStyleBoldCentered ?? (_labelStyleBoldCentered = new GUIStyle(CustomGUIStyles.Label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            });
        
        private static GUIStyle _labelStyleBoldRight;
        public static GUIStyle BoldLabelRight =>
            _labelStyleBoldRight ?? (_labelStyleBoldRight = new GUIStyle(CustomGUIStyles.Label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            });
        

        private static GUIStyle _labelStyleUnpadded;
        public static GUIStyle UnpaddedLabel =>
            _labelStyleUnpadded ?? (_labelStyleUnpadded = new GUIStyle(CustomGUIStyles.Label)
            {
                padding = new RectOffset(),
                fontStyle = FontStyle.Normal,
            });

        
        //_labelStyleUnpadded.font.material.color = Color.white;
        private static GUIStyle _miniLabelStyleLeft;
        public static GUIStyle MiniLabel =>
            _miniLabelStyleLeft ?? (_miniLabelStyleLeft = new GUIStyle("MiniLabel")
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                margin = new RectOffset(4, 4, 4, 4),
                normal =
                {
                    textColor = Color.grey
                }
            });

        
        private static GUIStyle _miniLabelStyleCentered;
        public static GUIStyle MiniLabelCentered =>
            _miniLabelStyleCentered ?? (_miniLabelStyleCentered = new GUIStyle(CustomGUIStyles.MiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        
        private static GUIStyle _miniLabelStyleRight;
        public static GUIStyle MiniLabelRight =>
            _miniLabelStyleRight ?? (_miniLabelStyleRight = new GUIStyle(CustomGUIStyles.MiniLabel)
            {
                alignment = TextAnchor.MiddleRight
            });

        #endregion //Label Styles

#region Clean Styles
        // =============================================================================================================
        // Clean styles
        private static GUIStyle _cleanStyle;
        public static GUIStyle Clean =>
            _cleanStyle ?? (_cleanStyle = new GUIStyle()
            {
                overflow = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
            });

        
        private static GUIStyle _cleanStyleTextField;
        public static GUIStyle CleanTextField =>
            _cleanStyleTextField ?? (_cleanStyleTextField = new GUIStyle("TextField")
            {
                overflow = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
            });

        
        private static GUIStyle _cleanLabelField;
        public static GUIStyle CleanLabelField =>
            _cleanLabelField ?? (_cleanLabelField = new GUIStyle("ControlLabel")
            {
                overflow = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
            });

        #endregion
 
#region Toolbar Styles

        // =============================================================================================================
        // Toolbar styles
        
        private static GUIStyle _toolbarBackground;
        public static GUIStyle ToolbarBackground =>
            _toolbarBackground ?? (_toolbarBackground = new GUIStyle("toolbar")
            {
                padding = new RectOffset(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
                fixedHeight = 0.0f,
            });

        
        private static GUIStyle _toolbarIconButtonStyle;
        public static GUIStyle ToolbarIconButton =>
            _toolbarIconButtonStyle ?? (_toolbarIconButtonStyle = new GUIStyle("IconButton")
            {
                padding = new RectOffset(2, 2, 2, 2)
            });

        
        private static GUIStyle _toggleGroupBackground;
        public static GUIStyle ToggleGroupBackground =>
            _toggleGroupBackground ?? (_toggleGroupBackground = new GUIStyle("HelpBox")
            {
                overflow = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(-2, -2, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            });

        private static GUIStyle _toggleGroupHeaderStyle;
        public static GUIStyle ToggleGroupHeader =>
            _toggleGroupHeaderStyle ?? (_toggleGroupHeaderStyle = new GUIStyle("RL Header")
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(2, 2, 0, 0)
            });

        
        private static GUIStyle _toggleGroupContentStyle;
        public static GUIStyle ToggleGroupContent =>
            _toggleGroupContentStyle ?? (_toggleGroupContentStyle = new GUIStyle(Clean)
            {
                padding = new RectOffset(2, 2, 0, 0)
            });
        
        private static GUIStyle _toolbarTabHeader;
        public static GUIStyle ToolbarTabHeader =>
            _toolbarTabHeader ?? (_toolbarTabHeader = new GUIStyle(ToggleGroupHeader)
            {
                margin = new RectOffset(-2, -2, 0, 0),
                padding = new RectOffset(2, 2, 0, 0),
            });
        
        private static GUIStyle _toolbarTabBackground;
        public static GUIStyle ToolbarTabBackground =>
            _toolbarTabBackground ?? (_toolbarTabBackground = new GUIStyle(ToggleGroupBackground)
            {
                margin = new RectOffset(-2, -2, 0, 0),
            });

        private static GUIStyle _toolbarTab;
        public static GUIStyle ToolbarTab =>
            _toolbarTab ?? (_toolbarTab = new GUIStyle("toolbarbutton")
            {
                fixedHeight = 0.0f,
                stretchHeight = true,
                stretchWidth = true
            });
        
        private static GUIStyle _toolbarCentered;
        public static GUIStyle ToolbarButtonCentered =>
            _toolbarCentered ?? (_toolbarCentered = new GUIStyle("toolbarbutton")
            {
                fixedHeight = 0.0f,
                alignment = TextAnchor.MiddleCenter,
                stretchHeight = true,
                stretchWidth = false
            });
        
        private static GUIStyle _toolbarTabButtons;
        public static GUIStyle ToolbarTabButtons =>
            _toolbarCentered ?? (_toolbarCentered = new GUIStyle("toolbarbutton")
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(2, 2, 0, 0),
            });
        
        private static GUIStyle _toolbarSearchTextField;
        public static GUIStyle ToolbarSearchTextField =>
#if UNITY_2023_1_OR_NEWER
            _toolbarSearchTextField ?? (_toolbarSearchTextField = new GUIStyle("ToolbarSearchTextField"));
#else
            _toolbarSearchTextField ?? (_toolbarSearchTextField = new GUIStyle("ToolbarSeachTextField"));
#endif

        
        private static GUIStyle _toolbarSearchCancelButton;
        public static GUIStyle ToolbarSearchCancelButton =>
#if UNITY_2023_1_OR_NEWER
            _toolbarSearchCancelButton ?? (_toolbarSearchCancelButton = new GUIStyle("ToolbarSearchCancelButton"));
#else
            _toolbarSearchCancelButton ?? (_toolbarSearchCancelButton = new GUIStyle("ToolbarSeachCancelButton"));
#endif

        #endregion

#region Button Styles
        // =============================================================================================================
        // Button Styles
        private static GUIStyle _iconButtonStyle;
        public static GUIStyle IconButton =>
            _iconButtonStyle ?? (_iconButtonStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(1, 1, 1, 1)
            });
        
        private static GUIStyle _commandButtonStyle;
        public static GUIStyle CommandButton => _commandButtonStyle ?? (_commandButtonStyle = new GUIStyle("Command")
        {
            fixedWidth = 28
        });

        
        private static GUIStyle _buttonStyle;
        public static GUIStyle Button => _buttonStyle ?? (_buttonStyle = new GUIStyle((GUIStyle)nameof(Button)));

        
        private static GUIStyle _buttonStyleSelected;
        public static GUIStyle ButtonSelected =>
            _buttonStyleSelected ?? (_buttonStyleSelected = new GUIStyle(CustomGUIStyles.Button)
            {
                normal = new GUIStyle(CustomGUIStyles.Button).onNormal
            });

        
        private static GUIStyle _buttonStyleLeft;
        public static GUIStyle ButtonLeft =>
            _buttonStyleLeft ?? (_buttonStyleLeft = GUI.skin.FindStyle(nameof(ButtonLeft), "button"));

        
        private static GUIStyle _iconButtonStyleLeft;
        public static GUIStyle IconButtonLeft =>
            _iconButtonStyleLeft ?? (_iconButtonStyleLeft = new GUIStyle(CustomGUIStyles.ButtonLeft)
            {
                padding = new RectOffset(1, 1, 1, 1)
            });

        
        private static GUIStyle _buttonStyleLeftSelected;
        public static GUIStyle ButtonLeftSelected =>
            _buttonStyleLeftSelected ?? (_buttonStyleLeftSelected = new GUIStyle(CustomGUIStyles.ButtonLeft)
            {
                normal = CustomGUIStyles.ButtonLeft.onNormal
            });

        
        private static GUIStyle _buttonStyleMiddle;
        public static GUIStyle ButtonMid =>
            _buttonStyleMiddle ?? (_buttonStyleMiddle = new GUIStyle(GUI.skin.FindStyle(nameof(ButtonMid), "button")));

        
        private static GUIStyle _buttonStyleMidSelected;
        public static GUIStyle ButtonMidSelected =>
            _buttonStyleMidSelected ?? (_buttonStyleMidSelected = new GUIStyle(CustomGUIStyles.ButtonMid)
            {
                normal = CustomGUIStyles.ButtonMid.onNormal
            });

        
        private static GUIStyle _iconButtonStyleMid;
        public static GUIStyle IconButtonMid =>
            _iconButtonStyleMid ?? (_iconButtonStyleMid = new GUIStyle(CustomGUIStyles.ButtonMid)
            {
                padding = new RectOffset(1, 1, 1, 1)
            });

        
        private static GUIStyle _buttonStyleRight;
        public static GUIStyle ButtonRight =>
            _buttonStyleRight ?? (_buttonStyleRight = GUI.skin.FindStyle(nameof(ButtonRight), "button"));

        
        private static GUIStyle _buttonStyleRightSelected;
        public static GUIStyle ButtonRightSelected =>
            _buttonStyleRightSelected ?? (_buttonStyleRightSelected =
                new GUIStyle(CustomGUIStyles.ButtonRight)
                {
                    normal = CustomGUIStyles.ButtonRight.onNormal
                });

        
        private static GUIStyle _iconButtonStyleRight;
        public static GUIStyle IconButtonRight =>
            _iconButtonStyleRight ?? (_iconButtonStyleRight = new GUIStyle(CustomGUIStyles.ButtonRight)
            {
                padding = new RectOffset(1, 1, 1, 1)
            });

        public static GUIStyle GetButtonGroupStyle(int buttonI, int maxI, bool selected = false)
        {
            if (buttonI == 0 && maxI <= 1)
                return selected ? CustomGUIStyles.ButtonSelected : CustomGUIStyles.Button;
            if (buttonI == 0) return selected ? CustomGUIStyles.ButtonLeftSelected : CustomGUIStyles.ButtonLeft;
            if (buttonI >= maxI - 1) return selected ? CustomGUIStyles.ButtonRightSelected : CustomGUIStyles.ButtonRight;
            return selected ? CustomGUIStyles.ButtonMidSelected : CustomGUIStyles.ButtonMid;
        }

        
        private static GUIStyle _miniButtonStyle;
        public static GUIStyle MiniButton => _miniButtonStyle ?? (_miniButtonStyle = new GUIStyle("miniButton"));

        
        private static GUIStyle _miniButtonStyleSelected;
        public static GUIStyle MiniButtonSelected =>
            _miniButtonStyleSelected ?? (_miniButtonStyleSelected = new GUIStyle(CustomGUIStyles.MiniButton)
            {
                normal = new GUIStyle(CustomGUIStyles.MiniButton).onNormal
            });

        
        private static GUIStyle _miniButtonStyleLeft;
        public static GUIStyle MiniButtonLeft => 
            _miniButtonStyleLeft ?? (_miniButtonStyleLeft = new GUIStyle("miniButtonLeft"));


        private static GUIStyle _miniButtonStyleLeftSelected;
        public static GUIStyle MiniButtonLeftSelected =>
            _miniButtonStyleLeftSelected ?? (_miniButtonStyleLeftSelected =
                new GUIStyle(CustomGUIStyles.MiniButtonLeft)
                {
                    normal = new GUIStyle(CustomGUIStyles.MiniButtonLeft).onNormal
                });

        
        private static GUIStyle _miniButtonStyleMid;
        public static GUIStyle MiniButtonMid =>
            _miniButtonStyleMid ?? (_miniButtonStyleMid = new GUIStyle("miniButtonMid"));

        private static GUIStyle _miniButtonStyleMidSelected;
        public static GUIStyle MiniButtonMidSelected =>
            _miniButtonStyleMidSelected ?? (_miniButtonStyleMidSelected =
                new GUIStyle(CustomGUIStyles.MiniButtonMid)
                {
                    normal = new GUIStyle(CustomGUIStyles.MiniButtonMid).onNormal
                });

        
        private static GUIStyle _miniButtonStyleRight;
        public static GUIStyle MiniButtonRight => 
            _miniButtonStyleRight ?? (_miniButtonStyleRight = new GUIStyle("miniButtonRight"));


        private static GUIStyle _miniButtonStyleRightSelected;
        public static GUIStyle MiniButtonRightSelected =>
            _miniButtonStyleRightSelected ?? (_miniButtonStyleRightSelected =
                new GUIStyle(CustomGUIStyles.MiniButtonRight)
                {
                    normal = new GUIStyle(CustomGUIStyles.MiniButtonRight).onNormal
                });

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