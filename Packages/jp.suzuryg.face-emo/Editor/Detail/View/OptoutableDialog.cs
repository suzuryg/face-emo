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

        private static Vector2 _scrollPosition;

        private static GUIStyle _labelStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _toggleStyle;

        public static bool Show(string title, string message, string ok,
            string cancel = null, string showDialogKey = null, bool showDialogDefaultValue = true,
            Vector2? centerPosition = null,
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

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.fontSize = FontSize;
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
            if (GUILayout.Button(_ok, _buttonStyle, GUILayout.MinWidth(ButtonWidth), GUILayout.Height(ButtonHeight)))
            {
                _result = true;
                Close();
            }
        }

        private void Cancel()
        {
            if (!string.IsNullOrEmpty(_cancel))
            {
                if (GUILayout.Button(_cancel, _buttonStyle, GUILayout.MinWidth(ButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    _result = false;
                    Close();
                }
            }
        }
    }
}
