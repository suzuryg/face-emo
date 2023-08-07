using Suzuryg.FaceEmo.Detail.Localization;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class OptoutableDialog : EditorWindow
    {
        public const int DefaultWindowWidth = 400;
        public const int DefaultWindowHeight = 150;

        private static readonly int Padding = 10;
        private static readonly int ButtonWidth = 100;
        private static readonly int ButtonHeight = 30;
        private static readonly int ButtonMargin = 10;
        private static readonly int FontSize = 13;

        private static bool? _isOkLeft = null;

        private static bool _result;
        private static string _message;
        private static string _ok;
        private static string _cancel;
        private static string _showDialogKey;
        private static bool _showDialogDefaultValue;
        private static bool _isRiskyAction;

        private static Vector2 _scrollPosition;

        private static GUIStyle _labelStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _normalButtonStyle;

        private static Texture2D _safeNormalTexture;
        private static Texture2D _safeHoverTexture;
        private static Texture2D _safeActiveTexture;

        private static Texture2D _riskyNormalTexture;
        private static Texture2D _riskyHoverTexture;
        private static Texture2D _riskyActiveTexture;

        private static GUIStyle _safeButtonStyle;
        private static GUIStyle _riskyButtonStyle;

        public static bool Show(string title, string message, string ok,
            string cancel = null, string showDialogKey = null, bool showDialogDefaultValue = true,
            Vector2? centerPosition = null,
            bool isRiskyAction = false,
            int windowWidth = DefaultWindowWidth, int windowHeight = DefaultWindowHeight)
        {
            // Check optout value
            if (!EditorPrefs.GetBool(showDialogKey, showDialogDefaultValue)) { return true; }

            // Initialize
            _result = false;
            _scrollPosition = Vector2.zero;

            // Set arguments
            _message = message;
            _ok = ok;
            _cancel = cancel;
            _showDialogKey = showDialogKey;
            _showDialogDefaultValue = showDialogDefaultValue;
            _isRiskyAction = isRiskyAction;

            // Get window
            var window = GetWindow<OptoutableDialog>();
            window.titleContent = new GUIContent(title);
            window.minSize = new Vector2(windowWidth, windowHeight);

            // Adjust size and position
            var pos = new Rect();
            pos.size = new Vector2(windowWidth, windowHeight);

            if (centerPosition.HasValue)
            {
                pos.x = centerPosition.Value.x - windowWidth / 2;
                pos.y = centerPosition.Value.y - windowHeight / 2;
            }
            else
            {
                var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                pos.x = mousePosition.x - windowWidth / 2;
                pos.y = mousePosition.y - windowHeight / 2;
            }

            if (pos.x + pos.width > Screen.currentResolution.width)
            {
                pos.x = Screen.currentResolution.width - pos.width;
            }
            if (pos.y + pos.height > Screen.currentResolution.height)
            {
                pos.y = Screen.currentResolution.height - pos.height;
            }
            window.position = pos;

            // Show
            window.ShowModalUtility();
            return _result;
        }

        private void GetStyles()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.fontSize = FontSize;
                _labelStyle.wordWrap = true;
            }

            if (_toggleStyle == null)
            {
                _toggleStyle = new GUIStyle(GUI.skin.toggle);
                _toggleStyle.fontSize = FontSize;
            }

            if (_normalButtonStyle == null)
            {
                _normalButtonStyle = new GUIStyle(GUI.skin.button);
                _normalButtonStyle.fontSize = FontSize;
            }

            SetTexture(ref _safeNormalTexture,  new Color(0.21f, 0.46f, 0.84f, 1f),  new Color(0.24f, 0.53f, 0.98f, 1f));
            SetTexture(ref _safeHoverTexture,   new Color(0.19f, 0.42f, 0.76f, 1f),  new Color(0.22f, 0.48f, 0.88f, 1f));
            SetTexture(ref _safeActiveTexture,  new Color(0.17f, 0.38f, 0.68f, 1f),  new Color(0.20f, 0.43f, 0.78f, 1f));

            SetTexture(ref _riskyNormalTexture, new Color(0.70f, 0.27f, 0.24f, 1f),  new Color(0.86f, 0.33f, 0.30f, 1f));
            SetTexture(ref _riskyHoverTexture,  new Color(0.60f, 0.23f, 0.21f, 1f),  new Color(0.76f, 0.29f, 0.26f, 1f));
            SetTexture(ref _riskyActiveTexture, new Color(0.50f, 0.19f, 0.17f, 1f),  new Color(0.66f, 0.25f, 0.22f, 1f));

            if (_safeButtonStyle == null)
            {
                _safeButtonStyle = new GUIStyle(GUI.skin.button);
                _safeButtonStyle.fontSize = FontSize;
                _safeButtonStyle.fontStyle = FontStyle.Bold;

                _safeButtonStyle.normal.textColor = Color.white;
                _safeButtonStyle.normal.background = _safeNormalTexture;
                _safeButtonStyle.normal.scaledBackgrounds = new[] { _safeNormalTexture };

                _safeButtonStyle.hover.textColor = Color.white;
                _safeButtonStyle.hover.background = _safeHoverTexture;
                _safeButtonStyle.hover.scaledBackgrounds = new[] { _safeHoverTexture };

                _safeButtonStyle.active.textColor = Color.white;
                _safeButtonStyle.active.background = _safeActiveTexture;
                _safeButtonStyle.active.scaledBackgrounds = new[] { _safeActiveTexture };
            }

            if (_riskyButtonStyle == null)
            {
                _riskyButtonStyle = new GUIStyle(GUI.skin.button);
                _riskyButtonStyle.fontSize = FontSize;
                _riskyButtonStyle.fontStyle = FontStyle.Bold;

                _riskyButtonStyle.normal.textColor = Color.white;
                _riskyButtonStyle.normal.background = _riskyNormalTexture;
                _riskyButtonStyle.normal.scaledBackgrounds = new[] { _riskyNormalTexture };

                _riskyButtonStyle.hover.textColor = Color.white;
                _riskyButtonStyle.hover.background = _riskyHoverTexture;
                _riskyButtonStyle.normal.scaledBackgrounds = new[] { _riskyHoverTexture };

                _riskyButtonStyle.active.textColor = Color.white;
                _riskyButtonStyle.active.background = _riskyActiveTexture;
                _riskyButtonStyle.active.scaledBackgrounds = new[] { _riskyActiveTexture };
            }
        }

        private static void SetTexture(ref Texture2D texture, Color dark, Color light)
        {
            if (texture == null)
            {
                if (EditorGUIUtility.isProSkin)
                {
                    texture = ViewUtility.MakeTexture(dark);
                }
                else
                {
                    texture = ViewUtility.MakeTexture(light);
                }
            }
        }

        private void OnGUI()
        {
            GetStyles();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(Padding);
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(Padding);

                    // message
                    using (var scope = new GUILayout.ScrollViewScope(_scrollPosition))
                    {
                        _scrollPosition = scope.scrollPosition;
                        GUILayout.Label(_message, _labelStyle);
                    }
                    GUILayout.FlexibleSpace();

                    // toggle
                    if (!string.IsNullOrEmpty(_showDialogKey))
                    {
                        var loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
                        var showDialog = EditorPrefs.GetBool(_showDialogKey, _showDialogDefaultValue);
                        var dontShowAgain = !showDialog;
                        if (GUILayout.Toggle(dontShowAgain, loc.Common_DoNotShowAgain, _toggleStyle) != dontShowAgain)
                        {
                            EditorPrefs.SetBool(_showDialogKey, !showDialog); // If changed, set inverse
                        }
                    }

                    // buttons
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (IsOkLeft())
                        {
                            OK();
                            GUILayout.Space(ButtonMargin);
                            Cancel();
                        }
                        else
                        {
                            Cancel();
                            GUILayout.Space(ButtonMargin);
                            OK();
                        }
                    }

                    GUILayout.Space(Padding);
                }
                GUILayout.Space(Padding);
            }
        }

        private bool IsOkLeft()
        {
            if (!_isOkLeft.HasValue)
            {
                var platform = System.Environment.OSVersion.Platform;
                if (platform == System.PlatformID.Unix || platform == System.PlatformID.MacOSX)
                {
                    _isOkLeft = false;
                }
                else
                {
                    _isOkLeft = true;
                }
            }
            return _isOkLeft.Value;
        }

        private void OK()
        {
            // FIX: Button color may not change to hover color on mouse over (occurs only when GUIStyle background is changed).
            var style = _isRiskyAction ? _riskyButtonStyle : _safeButtonStyle;
            if (GUILayout.Button(_ok, style, GUILayout.MinWidth(ButtonWidth), GUILayout.Height(ButtonHeight)))
            {
                _result = true;
                Close();
            }
        }

        private void Cancel()
        {
            if (!string.IsNullOrEmpty(_cancel))
            {
                if (GUILayout.Button(_cancel, _normalButtonStyle, GUILayout.MinWidth(ButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    _result = false;
                    Close();
                }
            }
        }
    }
}
