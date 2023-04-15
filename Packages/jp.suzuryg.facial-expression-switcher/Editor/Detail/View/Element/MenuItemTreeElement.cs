using Suzuryg.FacialExpressionSwitcher.Domain;
using Suzuryg.FacialExpressionSwitcher.Detail.AV3;
using Suzuryg.FacialExpressionSwitcher.Detail.Data;
using Suzuryg.FacialExpressionSwitcher.Detail.Drawing;
using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
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

namespace Suzuryg.FacialExpressionSwitcher.Detail.View.Element
{
    public class MenuItemTreeElement : TreeElementBase
    {
        private static readonly float TopMargin = 5;
        private static readonly float LeftMargin = 0;
        private static readonly float BottomMargin = 5;
        private static readonly float RightMargin = 10;
        private static readonly int Padding = 10;
        private static readonly Color SelectedRowColor = new Color(0f, 0.5f, 1f, 0.4f);

        public IObservable<(
            string modeId,
            string displayName,
            bool? useAnimationNameAsDisplayName,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? BlinkEnabled,
            bool? MouthMorphCancelerEnabled)> OnModePropertiesModified => _onModePropertiesModified.AsObservable();
        public IObservable<(string groupId, string displayName)> OnGroupPropertiesModified => _onGroupPropertiesModified.AsObservable();
        public IObservable<string> OnEnteredIntoGroup => _onEnteredIntoGroup.AsObservable();
        public IObservable<(string modeId, string clipGUID)> OnAnimationChanged => _onAnimationChanged.AsObservable();

        private Subject<(
            string modeId,
            string displayName,
            bool? useAnimationNameAsDisplayName,
            EyeTrackingControl? eyeTrackingControl,
            MouthTrackingControl? mouthTrackingControl,
            bool? BlinkEnabled,
            bool? MouthMorphCancelerEnabled)> _onModePropertiesModified = new Subject<(string modeId, string displayName, bool? useAnimationNameAsDisplayName, EyeTrackingControl? eyeTrackingControl, MouthTrackingControl? mouthTrackingControl, bool? BlinkEnabled, bool? MouthMorphCancelerEnabled)>();

        private Subject<(string groupId, string displayName)> _onGroupPropertiesModified = new Subject<(string groupId, string displayName)>();
        private Subject<string> _onEnteredIntoGroup = new Subject<string>();
        private Subject<(string modeId, string clipGUID)> _onAnimationChanged = new Subject<(string modeId, string clipGUID)>();

        private ModeNameProvider _modeNameProvider;
        private AnimationElement _animationElement;
        private MainThumbnailDrawer _thumbnailDrawer;
        private AV3Setting _aV3Setting;
        private MenuItemListViewState _menuItemListViewState;

        private bool _isLayoutInitialized = false;
        private float _previousThumbnailWidth = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailWidth) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailWidth) : DetailConstants.DefaultMainThumbnailWidth;
        private float _previousThumbnailHeight = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailHeight) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailHeight) : DetailConstants.DefaultMainThumbnailHeight;
        private Texture2D _selectedBackgroundTexture;

        private string _useAnimationNameText;
        private string _eyeTrackingText;
        private string _mouthTrackingText;
        private string _blinkText;
        private string _mouthMorphCancelerText;
        private string _emptyText;

        private Texture2D _folderIcon;

        private GUIStyle _itemStyle;
        private GUIStyle _emptyStyle;

        public MenuItemTreeElement(
            IReadOnlyLocalizationSetting localizationSetting,
            ModeNameProvider modeNameProvider,
            AnimationElement animationElement,
            MainThumbnailDrawer thumbnailDrawer,
            AV3Setting aV3Setting,
            MenuItemListViewState menuItemListViewState) : base(localizationSetting, menuItemListViewState.TreeViewState)
        {
            // Drawer
            _thumbnailDrawer = thumbnailDrawer;

            // State
            _menuItemListViewState = menuItemListViewState;
            if (_menuItemListViewState.RootGroupId is null)
            {
                _menuItemListViewState.RootGroupId = Domain.Menu.RegisteredId;
            }

            // Others
            _modeNameProvider = modeNameProvider;
            _animationElement = animationElement;
            _aV3Setting = aV3Setting;

            // Set icon
            _folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DetailConstants.IconDirectory}/folder_FILL0_wght400_GRAD200_opsz48.png");
            NullChecker.Check(_folderIcon);

            // Styles
            try
            {
                _itemStyle = new GUIStyle(EditorStyles.helpBox);
            }
            catch (NullReferenceException)
            {
                // Workaround for play mode
                _itemStyle = new GUIStyle();
            }
            _itemStyle.padding = new RectOffset(Padding, Padding, Padding, Padding);

            _emptyStyle = new GUIStyle();
            _emptyStyle.padding = new RectOffset(10, 10, 10, 10);

            // Textures
            _selectedBackgroundTexture = new Texture2D(1, 1);
            _selectedBackgroundTexture.SetPixel(0, 0, SelectedRowColor);
            _selectedBackgroundTexture.Apply();
        }

        public void ChangeRootGroup(string rootGroupId)
        {
            if (rootGroupId != _menuItemListViewState.RootGroupId && rootGroupId is string && Menu is IMenu &&
                (rootGroupId == Domain.Menu.RegisteredId || rootGroupId == Domain.Menu.UnregisteredId || Menu.ContainsGroup(rootGroupId)))
            {
                _menuItemListViewState.RootGroupId = rootGroupId;
                Reload();
                SetSelection(new List<int>());
            }
        }

        public override void OnGUI(Rect rect)
        {
            // Update thumbnails
            var animations = GetAnimations();
            foreach (var animation in animations)
            {
                _thumbnailDrawer.GetThumbnail(animation);
            }
            _thumbnailDrawer.Update();

            // Draw rows
            var rows = GetRows();
            if (rows is null || rows.Count == 0)
            {
                using (new EditorGUILayout.HorizontalScope(_emptyStyle))
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_emptyText);
                    GUILayout.FlexibleSpace();
                }
            }
            else
            {
                base.OnGUI(rect);
            }

            // To draw thumbnail ovarlay.
            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }

            // To update row height.
            var thumbnailWidth = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailWidth) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailWidth) : DetailConstants.DefaultMainThumbnailWidth;
            var thumbnailHeight = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailHeight) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailHeight) : DetailConstants.DefaultMainThumbnailHeight;
            if (thumbnailWidth != _previousThumbnailWidth || thumbnailHeight != _previousThumbnailHeight)
            {
                _previousThumbnailWidth = thumbnailWidth;
                _previousThumbnailHeight = thumbnailHeight;
                Reload();
            }
        }

        protected override void SetText(LocalizationTable localizationTable)
        {
            _useAnimationNameText = localizationTable.MenuItemListView_UseAnimationNameAsDisplayName;
            _eyeTrackingText = localizationTable.MenuItemListView_EyeTracking;
            _mouthTrackingText = localizationTable.MenuItemListView_MouthTracking;
            _blinkText = localizationTable.MenuItemListView_Blink;
            _mouthMorphCancelerText = localizationTable.MenuItemListView_MouthMorphCanceler;
            _emptyText = localizationTable.MenuItemListView_Empty;
            base.SetText(localizationTable);
        }

        protected override void DoubleClickedItem(int id)
        {
            var menuItemId = GetMenuItemId(id);
            if (Menu.ContainsGroup(menuItemId))
            {
                _onEnteredIntoGroup.OnNext(menuItemId);
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            if (Menu is null || _menuItemListViewState.RootGroupId is null)
            {
                return rows;
            }

            if (_menuItemListViewState.RootGroupId == Domain.Menu.RegisteredId)
            {
                AddChildren(Menu.Registered, root, rows);
            }
            else if (_menuItemListViewState.RootGroupId == Domain.Menu.UnregisteredId)
            {
                AddChildren(Menu.Unregistered, root, rows);
            }
            else if (Menu.ContainsGroup(_menuItemListViewState.RootGroupId))
            {
                var rootGroup = Menu.GetGroup(_menuItemListViewState.RootGroupId);
                AddChildren(rootGroup, root, rows);
            }

            return rows;
        }

        private void AddChildren (IMenuItemList menuItemList, TreeViewItem item, IList<TreeViewItem> rows)
        {
            foreach (var menuItemId in menuItemList.Order)
            {
                var type = menuItemList.GetType(menuItemId);

                if (type == MenuItemType.Group)
                {
                    var group = menuItemList.GetGroup(menuItemId);
                    var groupTree = new TreeViewItem { id = GetElementId(menuItemId), depth = -1 };
                    item.AddChild(groupTree);
                    rows.Add(groupTree);
                }
                else if (type == MenuItemType.Mode)
                {
                    var mode = menuItemList.GetMode(menuItemId);
                    var modeTree = new TreeViewItem { id = GetElementId(menuItemId), depth = -1 };
                    item.AddChild(modeTree);
                    rows.Add(modeTree);
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            // Workaround for initial layout problem
            const float minWidth = 10;
            if (Event.current.type == EventType.Layout)
            {
                if (args.rowRect.width >= minWidth)
                {
                    _isLayoutInitialized = true;
                }
                else
                {
                    Repaint();
                    return;
                }
            }
            else if (Event.current.type == EventType.Repaint && !_isLayoutInitialized)
            {
                Repaint();
                return;
            }

            // Draw background
            if (args.selected)
            {
                GUI.DrawTexture(args.rowRect, _selectedBackgroundTexture);
            }

            // Draw foreground
            var menuItemId = GetMenuItemId(args.item.id);
            try
            {
                if (Menu.ContainsMode(menuItemId))
                {
                    DrawMode(menuItemId, Menu.GetMode(menuItemId), args.rowRect);
                }
                else if (Menu.ContainsGroup(menuItemId))
                {
                    DrawGroup(menuItemId ,Menu.GetGroup(menuItemId), args.rowRect);
                }
            }
            catch (ArgumentException ex)
            {
                // Workaround for resize problem
                Repaint();
                Debug.LogException(ex);
            }
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var menuItemId = GetMenuItemId(item.id);
            if (Menu.ContainsMode(menuItemId))
            {
                return Math.Max(GetMinHeight(), AnimationElement.GetHeight()) + TopMargin + BottomMargin + Padding * 2;
            }
            else if (Menu.ContainsGroup(menuItemId))
            {
                return 80;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        private float GetMinHeight()
        {
            var line = EditorGUIUtility.singleLineHeight;
            return line + 10 + line + 10 + line + line;
        }

        private void DrawMode(string menuItemId, IMode mode, Rect rect)
        {
            using (new GUILayout.AreaScope(GetRowRectWithMargin(rect)))
            using (new EditorGUILayout.HorizontalScope(_itemStyle))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    // Display name
                    if (mode.UseAnimationNameAsDisplayName)
                    {
                        EditorGUILayout.LabelField(_modeNameProvider.Provide(mode));
                    }
                    else
                    {
                        var displayName = EditorGUILayout.DelayedTextField(mode.DisplayName);
                        if (displayName != mode.DisplayName)
                        {
                            _onModePropertiesModified.OnNext((menuItemId, displayName, null, null, null, null, null));
                        }
                    }

                    // Use animation name
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var useAnimationName = EditorGUILayout.Toggle(string.Empty, mode.UseAnimationNameAsDisplayName, GUILayout.Width(15));
                        if (useAnimationName != mode.UseAnimationNameAsDisplayName)
                        {
                            _onModePropertiesModified.OnNext((menuItemId, null, useAnimationName, null, null, null, null));
                        }
                        GUILayout.Label(_useAnimationNameText);
                    }

                    GUILayout.Space(10);

                    // Eye tracking
                    Func<EyeTrackingControl, bool> eyeToBool = (EyeTrackingControl eyeTrackingControl) => eyeTrackingControl == EyeTrackingControl.Tracking;
                    Func<bool, EyeTrackingControl> boolToEye = (bool value) => value ? EyeTrackingControl.Tracking : EyeTrackingControl.Animation;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var eyeTracking = EditorGUILayout.Toggle(string.Empty, eyeToBool(mode.EyeTrackingControl), GUILayout.Width(15));
                        if (eyeTracking != eyeToBool(mode.EyeTrackingControl))
                        {
                            _onModePropertiesModified.OnNext((menuItemId, null, null, boolToEye(eyeTracking), null, null, null));
                        }
                        GUILayout.Label(_eyeTrackingText);
                    }

                    // Blink
                    if (_aV3Setting.ReplaceBlink)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var blink = EditorGUILayout.Toggle(string.Empty, mode.BlinkEnabled, GUILayout.Width(15));
                            if (blink != mode.BlinkEnabled)
                            {
                                _onModePropertiesModified.OnNext((menuItemId, null, null, null, null, blink, null));
                            }
                            GUILayout.Label(_blinkText);
                        }
                    }
                    else
                    {
                        ViewUtility.LayoutDummyToggle(_blinkText);
                    }

                    // Mouth tracking
                    Func<MouthTrackingControl, bool> mouthToBool = (MouthTrackingControl mouthTrackingControl) => mouthTrackingControl == MouthTrackingControl.Tracking;
                    Func<bool, MouthTrackingControl> boolToMouth = (bool value) => value ? MouthTrackingControl.Tracking : MouthTrackingControl.Animation;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var mouthTracking = EditorGUILayout.Toggle(string.Empty, mouthToBool(mode.MouthTrackingControl), GUILayout.Width(15));
                        if (mouthTracking != mouthToBool(mode.MouthTrackingControl))
                        {
                            _onModePropertiesModified.OnNext((menuItemId, null, null, null, boolToMouth(mouthTracking), null, null));
                        }
                        GUILayout.Label(_mouthTrackingText);
                    }

                    // Mouth morph cancel
                    if (mouthToBool(mode.MouthTrackingControl))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var mouthMorphCancel = EditorGUILayout.Toggle(string.Empty, mode.MouthMorphCancelerEnabled, GUILayout.Width(15));
                            if (mouthMorphCancel != mode.MouthMorphCancelerEnabled)
                            {
                                _onModePropertiesModified.OnNext((menuItemId, null, null, null, null, null, mouthMorphCancel));
                            }
                            GUILayout.Label(_mouthMorphCancelerText);
                        }
                    }
                    else
                    {
                        ViewUtility.LayoutDummyToggle(_mouthMorphCancelerText);
                    }
                }

                GUILayout.Space(10);

                // Animation
                var thumbnailWidth = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailWidth) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailWidth) : DetailConstants.DefaultMainThumbnailWidth;
                var thumbnailHeight = EditorPrefs.HasKey(DetailConstants.KeyMainThumbnailHeight) ? EditorPrefs.GetInt(DetailConstants.KeyMainThumbnailHeight) : DetailConstants.DefaultMainThumbnailHeight;
                var animationRect = GUILayoutUtility.GetRect(new GUIContent(), new GUIStyle(), GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailHeight + EditorGUIUtility.singleLineHeight));
                _animationElement.Draw(animationRect, mode.Animation, _thumbnailDrawer,
                    guid => _onAnimationChanged.OnNext((menuItemId, guid)), _modeNameProvider.Provide(mode));
            }
        }

        private void DrawGroup(string menuItemId, IGroup group, Rect rect)
        {
            using (new GUILayout.AreaScope(GetRowRectWithMargin(rect)))
            using (new EditorGUILayout.HorizontalScope(_itemStyle))
            {
                // Icon
                var iconRect = GUILayoutUtility.GetRect(new GUIContent(), new GUIStyle(), GUILayout.Width(48), GUILayout.Height(48));
                GUI.DrawTexture(iconRect, _folderIcon);

                GUILayout.Space(10);

                // Display name
                var displayName = EditorGUILayout.DelayedTextField(group.DisplayName);
                if (displayName != group.DisplayName)
                {
                    _onGroupPropertiesModified.OnNext((menuItemId, displayName));
                }
            }
        }

        private Rect GetRowRectWithMargin(Rect rect)
        {
            return new Rect(
                rect.x + LeftMargin,
                rect.y + TopMargin,
                Math.Max(rect.width - LeftMargin - RightMargin, 0),
                Math.Max(rect.height - TopMargin - BottomMargin, 0));
        }

        private IMenuItemList GetRootMenuItemList()
        {
            var id = _menuItemListViewState.RootGroupId;

            if (id == Domain.Menu.RegisteredId)
            {
                return Menu.Registered;
            }
            else if (id == Domain.Menu.UnregisteredId)
            {
                return Menu.Unregistered;
            }
            else if (Menu.ContainsGroup(id))
            {
                return Menu.GetGroup(id);
            }
            else
            {
                return null;
            }
        }

        private List<Domain.Animation> GetAnimations()
        {
            List<Domain.Animation> animations = new List<Domain.Animation>();

            var parent = GetRootMenuItemList();
            if (parent is null) { return animations; }

            foreach (var id in parent.Order)
            {
                if (Menu.ContainsMode(id))
                {
                    animations.Add(Menu.GetMode(id).Animation);
                }
            }

            return animations;
        }
    }
}
