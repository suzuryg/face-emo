using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using Suzuryg.FaceEmo.Components.Settings;
using Suzuryg.FaceEmo.Detail.AV3;
using Suzuryg.FaceEmo.Detail.Drawing;
using Suzuryg.FaceEmo.Detail.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniRx;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class SettingView : IDisposable
    {
        private static readonly string DirtyMark = "*";

        private static GUIStyle _radioButtonStyle;

        private IModifyMenuPropertiesUseCase _modifyMenuPropertiesUseCase;
        private IGenerateFxUseCase _generateFxUseCase;

        private IGenerateFxPresenter _generateFxPresenter;

        private ILocalizationSetting _localizationSetting;
        private LocalizationTable _localizationTable;

        private ISubWindowProvider _subWindowProvider;
        private ModeNameProvider _modeNameProvider;
        private UpdateMenuSubject _updateMenuSubject;
        private MainThumbnailDrawer _thumbnailDrawer;
        private GestureTableThumbnailDrawer _gestureTableThumbnailDrawer;
        private SerializedObject _av3Setting;
        private SerializedObject _thumbnailSetting;

        private Label _thumbnailWidthLabel;
        private Label _thumbnailHeightLabel;
        private SliderInt _thumbnailWidthSlider;
        private SliderInt _thumbnailHeightSlider;
        private Button _updateThumbnailButton;
        private Label _defaultSelectionLabel;
        private IMGUIContainer _defaultSelectionComboBoxArea;
        private Toggle _showHintsToggle;
        private Button _openManualButton;
        private Image _openManualImage;
        private Label _openManualLabel;
        private Button _openOptionButton;
        private Image _openOptionImage;
        private Label _openOptionLabel;
        private IMGUIContainer _writeDefaultsSettingArea;
        private Button _applyButton;

        private Texture2D _bookIcon;
        private Texture2D _settingIcon;

        private string _unifyWriteDefaultsLabel;
        private string _disableWriteDefaultsLabel;
        private string _unifyWriteDefaultsTooltip;
        private string _disableWriteDefaultsTooltip;

        private List<ModeEx> _flattendModes = new List<ModeEx>();
        private string[] _modePaths = new string[0];
        private int _defaultSelection = 0;
        private string _applyButtonText = string.Empty;
        private EditorWindow _mainWindow;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SettingView(
            IModifyMenuPropertiesUseCase modifyMenuPropertiesUseCase,
            IGenerateFxUseCase generateFxUseCase,
            IGenerateFxPresenter generateFxPresenter,
            ILocalizationSetting localizationSetting,
            ISubWindowProvider subWindowProvider,
            ModeNameProvider modeNameProvider,
            MainWindowProvider mainWindowProvider,
            UpdateMenuSubject updateMenuSubject,
            MainThumbnailDrawer thumbnailDrawer,
            GestureTableThumbnailDrawer gestureTableThumbnailDrawer,
            AV3Setting av3Setting,
            ThumbnailSetting thumbnailSetting)
        {
            // Usecases
            _modifyMenuPropertiesUseCase = modifyMenuPropertiesUseCase;
            _generateFxUseCase = generateFxUseCase;

            // Presenters
            _generateFxPresenter = generateFxPresenter;

            // Others
            _localizationSetting = localizationSetting;
            _subWindowProvider = subWindowProvider;
            _modeNameProvider = modeNameProvider;
            _updateMenuSubject = updateMenuSubject;
            _thumbnailDrawer = thumbnailDrawer;
            _gestureTableThumbnailDrawer = gestureTableThumbnailDrawer;
            _av3Setting = new SerializedObject(av3Setting);
            _thumbnailSetting = new SerializedObject(thumbnailSetting);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(x => OnMenuUpdated(x.menu, x.isModified)).AddTo(_disposables);

            // Localization table changed event handler
            _localizationSetting.OnTableChanged.Synchronize().Subscribe(SetText).AddTo(_disposables);

            // Presenter event handlers
            _generateFxPresenter.Observable.Synchronize().Subscribe(OnGenerateFxPresenterCompleted).AddTo(_disposables);

            // MainWindow OnGUI event handler
            mainWindowProvider.OnGUI.Synchronize().ObserveOnMainThread().Subscribe(window => _mainWindow = window).AddTo(_disposables);

            // Initialization
            _bookIcon = ViewUtility.GetIconTexture("menu_book_FILL0_wght400_GRAD200_opsz48.png");
            _settingIcon = ViewUtility.GetIconTexture("settings_FILL0_wght400_GRAD200_opsz48.png");
        }

        public void Dispose()
        {
            _thumbnailWidthSlider.UnregisterValueChangedCallback(OnThumbnailSettingChanged);
            _thumbnailHeightSlider.UnregisterValueChangedCallback(OnThumbnailSettingChanged);
            _showHintsToggle.UnregisterValueChangedCallback(OnShowHintsValueChanged);

            _disposables.Dispose();
        }

        public void Initialize(VisualElement root)
        {
            // Load UXML and style
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{DetailConstants.ViewDirectory}/{nameof(SettingView)}.uxml");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{DetailConstants.ViewDirectory}/{nameof(SettingView)}.uss");
            NullChecker.Check(uxml, style);

            root.styleSheets.Add(style);
            uxml.CloneTree(root);

            // Query Elements
            _thumbnailWidthLabel = root.Q<Label>("ThumbnailWidthLabel");
            _thumbnailHeightLabel = root.Q<Label>("ThumbnailHeightLabel");
            _thumbnailWidthSlider = root.Q<SliderInt>("ThumbnailWidthSlider");
            _thumbnailHeightSlider = root.Q<SliderInt>("ThumbnailHeightSlider");
            _updateThumbnailButton = root.Q<Button>("UpdateThumbnailButton");
            _defaultSelectionLabel = root.Q<Label>("DefaultSelectionLabel");
            _defaultSelectionComboBoxArea = root.Q<IMGUIContainer>("DefaultSelectionComboBox");
            _showHintsToggle = root.Q<Toggle>("ShowHintsToggle");
            _openManualButton = root.Q<Button>("OpenManualButton");
            _openManualImage = root.Q<Image>("OpenManualImage");
            _openManualLabel = root.Q<Label>("OpenManualLabel");
            _openOptionButton = root.Q<Button>("OpenOptionButton");
            _openOptionImage = root.Q<Image>("OpenOptionImage");
            _openOptionLabel = root.Q<Label>("OpenOptionLabel");
            _writeDefaultsSettingArea = root.Q<IMGUIContainer>("WriteDefaultsSetting");
            _applyButton = root.Q<Button>("ApplyButton");
            NullChecker.Check(_thumbnailWidthLabel, _thumbnailHeightLabel, _thumbnailWidthSlider, _thumbnailHeightSlider, _updateThumbnailButton,
               _defaultSelectionLabel, _defaultSelectionComboBoxArea, _showHintsToggle,
               _openManualButton, _openManualImage, _openManualLabel, _openOptionButton, _openOptionImage, _openOptionLabel,
               _writeDefaultsSettingArea, _applyButton);

            // Initialize fields
            _thumbnailSetting.Update();

            _thumbnailWidthSlider.lowValue = ThumbnailSetting.Main_MinWidth;
            _thumbnailWidthSlider.highValue = ThumbnailSetting.Main_MaxWidth;
            _thumbnailWidthSlider.value = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_Width)).intValue;

            _thumbnailWidthSlider.bindingPath = nameof(ThumbnailSetting.Main_Width);
            _thumbnailWidthSlider.BindProperty(_thumbnailSetting);

            _thumbnailHeightSlider.lowValue = ThumbnailSetting.Main_MinHeight;
            _thumbnailHeightSlider.highValue = ThumbnailSetting.Main_MaxHeight;
            _thumbnailHeightSlider.value = _thumbnailSetting.FindProperty(nameof(ThumbnailSetting.Main_Height)).intValue;

            _thumbnailHeightSlider.bindingPath = nameof(ThumbnailSetting.Main_Height);
            _thumbnailHeightSlider.BindProperty(_thumbnailSetting);

            var showHints = EditorPrefs.HasKey(DetailConstants.KeyShowHints) ? EditorPrefs.GetBool(DetailConstants.KeyShowHints) : DetailConstants.DefaultShowHints;
            _showHintsToggle.value = showHints;

            _openManualImage.image = _bookIcon;
            _openManualImage.scaleMode = ScaleMode.ScaleToFit;
            _openOptionImage.image = _settingIcon;
            _openOptionImage.scaleMode = ScaleMode.ScaleToFit;

            // Add event handlers
            // Delay event registration due to unstable slider values immediately after opening the window.
            Observable.Timer(TimeSpan.FromMilliseconds(100)).ObserveOnMainThread().Subscribe(_ =>
            {
                _thumbnailWidthSlider.RegisterValueChangedCallback(OnThumbnailSettingChanged);
                _thumbnailHeightSlider.RegisterValueChangedCallback(OnThumbnailSettingChanged);
            }).AddTo(_disposables);
            _showHintsToggle.RegisterValueChangedCallback(OnShowHintsValueChanged);

            Observable.FromEvent(x => _updateThumbnailButton.clicked += x, x => _updateThumbnailButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnUpdateThumbnailButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _applyButton.clicked += x, x => _applyButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnApplyButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _defaultSelectionComboBoxArea.onGUIHandler += x, x => _defaultSelectionComboBoxArea.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => DefaultSelectionComboBoxOnGUI()).AddTo(_disposables);
            Observable.FromEvent(x => _openManualButton.clicked += x, x => _openManualButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnOpenManualButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _openOptionButton.clicked += x, x => _openOptionButton.clicked -= x)
                .Synchronize().Subscribe(_ => OnOpenOptionButtonClicked()).AddTo(_disposables);
            Observable.FromEvent(x => _writeDefaultsSettingArea.onGUIHandler += x, x => _writeDefaultsSettingArea.onGUIHandler -= x)
                .Synchronize().Subscribe(_ => WriteDefaultsSettingOnGUI()).AddTo(_disposables);

            // Set text
            SetText(_localizationSetting.Table);
        }

        private void SetText(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;

            if (_thumbnailWidthLabel != null) { _thumbnailWidthLabel.text = localizationTable.Common_ThumbnailWidth; }
            if (_thumbnailHeightLabel != null) { _thumbnailHeightLabel.text = localizationTable.Common_ThumbnailHeight; }
            if (_updateThumbnailButton != null) { _updateThumbnailButton.text = localizationTable.SettingView_UpdateThumbnails; }

            if (_defaultSelectionLabel != null)
            {
                _defaultSelectionLabel.text = localizationTable.SettingView_DefaultSelectedMode;
                _defaultSelectionLabel.tooltip = localizationTable.SettingView_Tooltip_DefaultSelectedMode;
            }

            if (_showHintsToggle != null) { _showHintsToggle.text = localizationTable.SettingView_ShowHints; }
            if (_openManualLabel != null) { _openManualLabel.text = localizationTable.SettingView_Manual; }
            if (_openOptionLabel != null) { _openOptionLabel.text = localizationTable.SettingView_Option; }

            _unifyWriteDefaultsLabel = localizationTable.SettingView_UnifyWriteDefaults;
            _disableWriteDefaultsLabel = localizationTable.SettingView_DisableWriteDefaults;
            _unifyWriteDefaultsTooltip = LocalizationSetting.InsertLineBreak(localizationTable.SettingView_Tooltip_UnifyWriteDefaults);
            _disableWriteDefaultsTooltip = LocalizationSetting.InsertLineBreak(localizationTable.SettingView_Tooltip_DisableWriteDefaults);

            _applyButtonText = localizationTable.SettingView_ApplyToAvatar;
            if (_applyButton != null)
            {
                var isDirty = _applyButton.text.Contains(DirtyMark);
                _applyButton.text = _applyButtonText;
                if (isDirty) { _applyButton.text += DirtyMark; }
                _applyButton.MarkDirtyRepaint();
            }
        }

        private void OnMenuUpdated(IMenu menu, bool isModified)
        {
            _flattendModes = AV3Utility.FlattenMenuItemList(menu.Registered, _modeNameProvider);
            _modePaths = _flattendModes.Select(x => x.PathToMode).ToArray();

            // Change apply button style
            if (isModified && _applyButton != null)
            {
                _applyButton.text = _applyButtonText + DirtyMark;
                _applyButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                _applyButton.MarkDirtyRepaint();
            }

            // Rename to avoid name duplication (for Popup)
            var used = new HashSet<string>();
            for (int i = 0; i < _modePaths.Length; i++)
            {
                while (used.Contains(_modePaths[i])) { _modePaths[i] = AddNumberToName(_modePaths[i]); }
                used.Add(_modePaths[i]);
            }

            // Update default selection
            _defaultSelection = 0;
            for (int i = 0; i < _flattendModes.Count; i++)
            {
                if (menu.DefaultSelection == _flattendModes[i].Mode.GetId())
                {
                    _defaultSelection = i;
                    break;
                }
            }
        }   

        private string AddNumberToName(string input)
        {
            const string pattern = @"\((\d+)\)$";
            var match = Regex.Match(input, pattern);
            if (match.Success)
            {
                int currentNumber = int.Parse(match.Groups[1].Value);
                int nextNumber = currentNumber + 1;
                return Regex.Replace(input, pattern, $"({nextNumber})");
            }
            else
            {
                return input + "(1)";
            }
        }

        private void OnThumbnailSettingChanged<T>(ChangeEvent<T> changeEvent)
        {
            // TODO: Reduce unnecessary redrawing
            _thumbnailDrawer.RequestUpdateAll();
        }

        private void OnUpdateThumbnailButtonClicked()
        {
            _thumbnailDrawer.RequestUpdateAll();
            _gestureTableThumbnailDrawer.RequestUpdateAll();
        }

        private void OnShowHintsValueChanged(ChangeEvent<bool> changeEvent)
        {
            EditorPrefs.SetBool(DetailConstants.KeyShowHints, changeEvent.newValue);
        }

        private void OnApplyButtonClicked()
        {
            if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.Common_Message_NotPossibleInPlayMode, "OK"); return; }

            var dialogWidth = 550;
            var dialogHeight = 150;

            if (_localizationSetting.Locale == Locale.en_US)
            {
                dialogHeight += (int)EditorGUIUtility.singleLineHeight;
            }

            if (OptoutableDialog.Show(DomainConstants.SystemName,
                LocalizationSetting.InsertLineBreak(_localizationTable.SettingView_Message_ConfirmApplyToAvatar),
                _localizationTable.Common_Apply, _localizationTable.Common_Cancel,
                windowWidth: dialogWidth, windowHeight: dialogHeight,
                centerPosition: GetDialogCenterPosition()))
            {
                List<string> editablePrefabPaths;
                try
                {
                    editablePrefabPaths = _generateFxUseCase.Prepare();
                }
                catch (Exception ex)
                {
                    editablePrefabPaths = new List<string>();
                    EditorUtility.DisplayDialog(DomainConstants.SystemName, _localizationTable.SettingView_Message_FailedObtainPrefabs
                        + "\n\n" + ex?.Message, "OK");
                    Debug.LogError(_localizationTable.SettingView_Message_FailedObtainPrefabs + "\n" + ex?.ToString());
                }

                var messages = new List<(MessageType type, string message)>()
                {
                    (MessageType.None, _localizationTable.SettingView_Message_ReplaceFaceEmoPrefab.Replace("<0>", AV3Constants.MARootObjectName)),
                    (MessageType.None, string.Empty),
                };
                foreach (var path in editablePrefabPaths)
                {
                    messages.Add((MessageType.None, path));
                }

                if (!editablePrefabPaths.Any() ||
                    OptoutableDialog.Show(DomainConstants.SystemName, string.Empty,
                    _localizationTable.Common_Proceed, _localizationTable.Common_Cancel,
                    DetailConstants.KeyEditPrefabsConfirmation, DetailConstants.DefaultEditPrefabsConfirmation,
                    isRiskyAction: false, additionalMessages: messages,
                    windowWidth: dialogWidth, windowHeight: OptoutableDialog.GetHeightWithoutMessage(), centerPosition: GetDialogCenterPosition()))
                {
                    _mainWindow?.Focus(); // DisplayProgressBar() is displayed in the center of the focused EditorWindow.
                    _generateFxUseCase.Handle("", editablePrefabPaths);
                }
            }
        }

        private void OnGenerateFxPresenterCompleted(
            (GenerateFxResult generateFxResult, string errorMessage) args)
        {
            if (args.generateFxResult == GenerateFxResult.Succeeded)
            {
                OptoutableDialog.Show(DomainConstants.SystemName, LocalizationSetting.InsertLineBreak(_localizationTable.SettingView_Message_Succeeded), "OK",
                    centerPosition: GetDialogCenterPosition());

                // Change apply button style
                if (_applyButton != null)
                {
                    _applyButton.text = _applyButtonText;
                    _applyButton.style.unityFontStyleAndWeight = FontStyle.Normal;
                    _applyButton.MarkDirtyRepaint();
                }
            }
        }

        private void DefaultSelectionComboBoxOnGUI()
        {
            var selected = EditorGUILayout.Popup(_defaultSelection, _modePaths);
            if (selected != _defaultSelection &&
                selected < _flattendModes.Count)
            {
                _modifyMenuPropertiesUseCase.Handle(string.Empty, _flattendModes[selected].Mode.GetId());
            }
        }

        private Vector2 GetDialogCenterPosition()
        {
            if (_mainWindow != null)
            {
                return GUIUtility.GUIToScreenPoint(new Vector2(_mainWindow.position.width / 2, _mainWindow.position.height / 2));
            }
            else
            {
                return GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            }
        }

        private void OnOpenManualButtonClicked()
        {
            var homeUrl = "https://suzuryg.github.io/face-emo/";
            var pageUrl = "docs/tutorials/";
            var fullUrl = homeUrl;
            if (_localizationSetting.Locale == Locale.ja_JP)
            {
                fullUrl += "ja/";
            }
            fullUrl += pageUrl;
            Application.OpenURL(fullUrl);
        }

        private void OnOpenOptionButtonClicked()
        {
            _subWindowProvider.Provide<InspectorWindow>()?.Focus();
        }

        private void WriteDefaultsSettingOnGUI()
        {
            if (_radioButtonStyle == null)
            {
                try
                {
                    _radioButtonStyle = new GUIStyle(EditorStyles.radioButton);
                }
                catch (NullReferenceException)
                {
                    // Workaround for play mode
                    _radioButtonStyle = new GUIStyle();
                }
                var padding = _radioButtonStyle.padding;
                _radioButtonStyle.padding = new RectOffset(padding.left + 3, padding.right, padding.top, padding.bottom);
            }

            var options = new[]
            {
                new GUIContent(_unifyWriteDefaultsLabel, _unifyWriteDefaultsTooltip),
                new GUIContent(_disableWriteDefaultsLabel, _disableWriteDefaultsTooltip),
            };

            _av3Setting.Update();
            var wdProperty = _av3Setting.FindProperty(nameof(AV3Setting.MatchAvatarWriteDefaults));
            var wdIndex = wdProperty.boolValue ? 0 : 1;

            var newValue = GUILayout.SelectionGrid(wdIndex, options, 1, _radioButtonStyle);
            if (newValue != wdIndex)
            {
                wdProperty.boolValue = !wdProperty.boolValue;
                _av3Setting.ApplyModifiedProperties();
            }
        }
    }
}
