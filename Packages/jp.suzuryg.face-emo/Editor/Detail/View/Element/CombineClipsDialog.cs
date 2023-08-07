using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.IMGUI.Controls;
using UniRx;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Localization;

namespace Suzuryg.FaceEmo.Detail.View.Element
{
    public class CombineClipsDialog : EditorWindow, ISubWindow
    {
        private static readonly int Margin = 5;
        private static readonly int ButtonWidth = 100;
        private static readonly int ButtonHeight = 30;
        private static readonly int FontSize = 13;

        public bool IsInitialized { get; set; } = true;

        private static AnimationElement _animationElement;
        private static MainThumbnailDrawer _mainThumbnailDrawer;
        private static int _thumbnailWidth;
        private static int _thumbnailHeight;
        private static AV3.ExpressionEditor _expressionEditor;
        private static Action<string> _setAnimationClipAction;

        private static Domain.Animation _leftAnimation;
        private static Domain.Animation _rightAnimation;

        private static bool? _isOkLeft = null;

        private static string _combine;
        private static string _cancel;

        private static GUIStyle _normalButtonStyle;
        private static GUIStyle _safeButtonStyle;

        private static Texture2D _safeNormalTexture;
        private static Texture2D _safeHoverTexture;
        private static Texture2D _safeActiveTexture;

        private static CompositeDisposable _disposables = new CompositeDisposable();

        public static void Show(AnimationElement animationElement, MainThumbnailDrawer mainThumbnailDrawer,
            int thumbnailWidth, int thumbnailHeight,
            AV3.ExpressionEditor expressionEditor, Action<string> setAnimationClipAction,
            Domain.Animation leftAnimation = null, Domain.Animation rightAnimation = null)
        {
            // Initialize
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            var loc = LocalizationSetting.GetTable(LocalizationSetting.GetLocale());
            _combine = loc.Common_Combine;
            _cancel = loc.Common_Cancel;

            // Set arguments
            _animationElement = animationElement;
            _mainThumbnailDrawer = mainThumbnailDrawer;
            _thumbnailWidth = thumbnailWidth;
            _thumbnailHeight = thumbnailHeight;
            _expressionEditor = expressionEditor;
            _setAnimationClipAction = setAnimationClipAction;

            _leftAnimation = leftAnimation;
            _rightAnimation = rightAnimation;

            // Get window
            var window = GetWindow<CombineClipsDialog>();
            window.titleContent = new GUIContent(DomainConstants.SystemName);
            window.wantsMouseMove = true;

            mainThumbnailDrawer.OnThumbnailUpdated.Synchronize().ObserveOnMainThread().Subscribe(_ =>
            {
                if (window != null) { window.Repaint(); }
            }).AddTo(_disposables);

            var windowWidth = Margin + _thumbnailWidth + Margin + _thumbnailWidth + Margin;
            var windowHeight = Margin + _thumbnailHeight + EditorGUIUtility.singleLineHeight + Margin + ButtonHeight + Margin;
            window.minSize = new Vector2(windowWidth, windowHeight);

            // Adjust size and position
            var pos = new Rect();
            pos.size = new Vector2(windowWidth, windowHeight);

            var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            pos.x = mousePosition.x - windowWidth / 2;
            pos.y = mousePosition.y - windowHeight / 2;

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
            window.Show();
            return;
        }

        private void GetStyles()
        {
            if (_normalButtonStyle == null)
            {
                _normalButtonStyle = new GUIStyle(GUI.skin.button);
                _normalButtonStyle.fontSize = FontSize;
            }

            SetTexture(ref _safeNormalTexture,  new Color(0.21f, 0.46f, 0.84f, 1f),  new Color(0.24f, 0.53f, 0.98f, 1f));
            SetTexture(ref _safeHoverTexture,   new Color(0.19f, 0.42f, 0.76f, 1f),  new Color(0.22f, 0.48f, 0.88f, 1f));
            SetTexture(ref _safeActiveTexture,  new Color(0.17f, 0.38f, 0.68f, 1f),  new Color(0.20f, 0.43f, 0.78f, 1f));

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

            _animationElement.Draw(new Rect(Margin, Margin, _thumbnailWidth, _thumbnailHeight + EditorGUIUtility.singleLineHeight),
                _leftAnimation,
                _mainThumbnailDrawer,
                guid => { _leftAnimation = new Domain.Animation(guid); },
                canCombine: false);

            _animationElement.Draw(new Rect(Margin + _thumbnailWidth + Margin, Margin, _thumbnailWidth, _thumbnailHeight + EditorGUIUtility.singleLineHeight),
                _rightAnimation,
                _mainThumbnailDrawer,
                guid => { _rightAnimation = new Domain.Animation(guid); },
                canCombine: false);

            var leftButtonRect = new Rect(
                Margin + _thumbnailWidth + Margin + _thumbnailWidth - ButtonWidth - Margin - ButtonWidth,
                Margin + _thumbnailHeight + EditorGUIUtility.singleLineHeight + Margin,
                ButtonWidth, ButtonHeight);

            var rightButtonRect = new Rect(
                Margin + _thumbnailWidth + Margin + _thumbnailWidth - ButtonWidth,
                Margin + _thumbnailHeight + EditorGUIUtility.singleLineHeight + Margin,
                ButtonWidth, ButtonHeight);

            if (IsOkLeft())
            {
                OK(leftButtonRect);
                Cancel(rightButtonRect);
            }
            else
            {
                Cancel(leftButtonRect);
                OK(rightButtonRect);
            }

            // To draw thumbnail ovarlay.
            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
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

        private void OK(Rect rect)
        {
            if (GUI.Button(rect, _combine, _safeButtonStyle))
            {
                var newClipGuid = _animationElement.GetAnimationGuidWithDialog(AnimationElement.DialogMode.Create, string.Empty, defaultClipName: null);
                if (string.IsNullOrEmpty(newClipGuid)) { return; }

                var newClipPath = AssetDatabase.GUIDToAssetPath(newClipGuid);
                var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath);

                var leftClip = AV3Utility.GetAnimationClipWithName(_leftAnimation).clip;
                var rightClip = AV3Utility.GetAnimationClipWithName(_rightAnimation).clip;

                AV3Utility.CombineExpressions(leftClip, rightClip, newClip);

                _expressionEditor.Open(newClip);
                _setAnimationClipAction(newClipGuid);

                _disposables?.Dispose();
                _disposables = new CompositeDisposable();
                Close();
            }
        }

        private void Cancel(Rect rect)
        {
            if (GUI.Button(rect, _cancel, _normalButtonStyle))
            {
                _disposables?.Dispose();
                _disposables = new CompositeDisposable();
                Close();
            }
        }
    }
}
