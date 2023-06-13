using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Localization
{
    public class LocalizationTable : ScriptableObject
    {
        public string Common_Add = "Add";
        public string Common_Cancel = "Cancel";
        public string Common_Delete = "Delete";
        public string Common_Clear = "Clear";
        public string Common_Yes = "Yes";
        public string Common_No = "No";

        public string Common_Neutral = "Neutral";
        public string Common_Fist = "Fist";
        public string Common_HandOpen = "Open";
        public string Common_Fingerpoint = "Point";
        public string Common_Victory = "Victory";
        public string Common_RockNRoll = "Rock";
        public string Common_HandGun = "Gun";
        public string Common_ThumbsUp = "Thumbs";

        public string Common_ThumbnailWidth = "Thumbnail Width";
        public string Common_ThumbnailHeight = "Thumbnail Height";
        public string Common_AddtionalToggleObjects = "Addtional Expression Objects (Toggle)";
        public string Common_AddtionalTransformObjects = "Addtional Expression Objects (Transform)";
        public string Common_Message_InvalidDestination = "You cannot move to here. Check the number of expression patterns and folders.";
        public string Common_Message_DeleteGroup = "Are you sure you want to delete the following folder?";
        public string Common_Message_DeleteMode = "Are you sure you want to delete the following expression pattern?";
        public string Common_Message_DeleteBranch = "Are you sure you want to delete the expression?";
        public string Common_Message_ClearMouthMorphBlendShapes = "Are you sure you want to clear mouth morph blend shapes?";
        public string Common_Message_NotPossibleInPlayMode = "This operation is not possible during Play mode.";
        public string Common_Message_GuidWasNotFound = "The selected asset does not exist in this project.";
        public string Common_Tooltip_DeleteBranch = "Deletes this expression.";
        public string Common_Tooltip_EyeTracking = "You can set whether to use pseudo eye-tracking. This setting can be configured independently of the blinking setting.";
        public string Common_Tooltip_Blink = "You can set whether to blink. Please disable blinking for expressions where the eyes are closed.";
        public string Common_Tooltip_LipSync = "You can set whether to use lip-syncing. " +
            "To prevent disruption of the expression during speech, please disable lip-syncing or use mouth deformation cancellation.";
        public string Common_Tooltip_MouthMorphCanceler = "During speech, you can prevent disruptions by resetting certain shape key values to their default values (the values set on the scene). " +
            "Please specify the shape keys to be reset to their default values in the \"MouthMorphBlendShapes\" in the Inspector.";
        public string Common_Tooltip_LeftTrigger = "Expressions change according to the pressure level of the left hand trigger. " +
            "Please set an additional expression for when the trigger is pressed. " +
            "Trigger pressing is only valid when the left hand is in Fist.";
        public string Common_Tooltip_RightTrigger = "Expressions change according to the pressure level of the right hand trigger. " +
            "Please set an additional expression for when the trigger is pressed. " +
            "Trigger pressing is only valid when the right hand is in Fist.";
        public string Common_Tooltip_LaunchFromHierarchy = "Launch FacialExpressionSwitcher";

        public string HierarchyView_Title = "Tree View";
        public string HierarchyView_RegisteredMenuItemList = "Expression Menu";
        public string HierarchyView_UnregisteredMenuItemList = "Archive";
        public string HierarchyView_Message_CanNotRename = "The \"Use animation name as pattern name\" setting is enabled. Disable the setting if you want to change the pattern name.";
        public string HierarchyView_Tooltip_AddMode = "Create a new expression pattern in the selected hierarchy";
        public string HierarchyView_Tooltip_AddGroup = "Create a new folder in the selected hierarchy";
        public string HierarchyView_Tooltip_Copy = "Copy the selected expression pattern or folder in the same hierarchy";
        public string HierarchyView_Tooltip_Delete = "Delete the selected expression pattern or folder";

        public string MenuItemListView_Title = "Expression Patterns";
        public string MenuItemListView_ChangeDefaultFace = "Change Default Face";
        public string MenuItemListView_UseAnimationNameAsDisplayName = "Use ClipName As PatternName";
        public string MenuItemListView_Tooltip_AddMode = "Create a new expression pattern in the current hierarchy";
        public string MenuItemListView_Tooltip_AddGroup = "Create a new folder in the current hierarchy";
        public string MenuItemListView_Tooltip_Copy = "Copy the selected expression pattern or folder in the same hierarchy";
        public string MenuItemListView_Tooltip_Delete = "Delete the selected expression pattern or folder";
        public string MenuItemListView_Tooltip_ClickAddressBar = "You can click here to change the level of detail being displayed.";
        public string MenuItemListView_Tooltip_DoubleClickFolder = "You can display the contents of a folder by double-clicking it.";
        public string MenuItemListView_Tooltip_ChangeDefaultFace = "You can change the default face (the expression when there's no expression change made by gestures).";
        public string MenuItemListView_Tooltip_UseAnimationNameAsDisplayName = "The name of the set animation clip is used as the name of the expression pattern.";

        public string MenuItemListView_EyeTracking = "Enable Eye Tracking";
        public string MenuItemListView_MouthTracking = "Enable Lip Sync";
        public string MenuItemListView_Blink = "Enable Blink";
        public string MenuItemListView_MouthMorphCanceler = "Mouth Morph Canceler";
        public string MenuItemListView_Empty = "You can add an expression pattern by pressing the button\nat the top of the view.";

        public string BranchListView_Title = "Expressions";
        public string BranchListView_Simplify = "Simplify";
        public string BranchListView_OpenGestureTable = "Open Gesture Table";
        public string BranchListView_ModeIsNotSelected = "Please select an expression pattern on the left panel.";
        public string BranchListView_UseLeftTrigger = "Use Left Trigger";
        public string BranchListView_UseRightTrigger = "Use Right Trigger";
        public string BranchListView_NotReachableBranch = "This expression is not used.";
        public string BranchListView_NotReachableBranchAction = "Modify the conditions or order of the expression.";
        public string BranchListView_LeftTriggerAnimation = "Left Trigger";
        public string BranchListView_RightTriggerAnimation = "Right Trigger";
        public string BranchListView_BothTriggersAnimation = "Both Triggers";
        public string BranchListView_EmptyBranch = "Press the + button to add a facial expression.";
        public string BranchListView_EmptyCondition = "Press the + button to add a condition.";
        public string BranchListView_Condition = "Conditions";
        public string BranchListView_Left = "LeftHand";
        public string BranchListView_Right = "RightHand";
        public string BranchListView_OneSide = "OneHand";
        public string BranchListView_Either = "AnyHand(s)";
        public string BranchListView_Both = "BothHands";
        public string BranchListView_Equals = "Equals";
        public string BranchListView_NotEqual = "NotEqual";
        public string BranchListView_Preset = "Preset";
        public string BranchListView_Preset_LeftOnly = "Left Only (7 Patterns)";
        public string BranchListView_Preset_RightOnly = "Right Only (7 Patterns)";
        public string BranchListView_Preset_LeftPriority = "Left Priority (14 Patterns)";
        public string BranchListView_Preset_RightPriority = "Right Priority (14 Patterns)";
        public string BranchListView_Preset_Combination = "Combination (63 Patterns)";
        public string BranchListView_Message_AddPreset = "Are you sure you want to add the following facial expression preset?";
        public string BranchListView_Message_InvalidPreset = "An error occurred while adding a facial expression preset.";
        public string BranchListView_Tooltip_AddPreset = "Several facial expressions can be added at once by selecting a preset.";
        public string BranchListView_Tooltip_OpenGestureTable = "You can confirm which expression corresponds to each combination of gestures.";
        public string BranchListView_Tooltip_Simplify = "Switches to a simple display of only the expression conditions and animation name. " +
            "Use this when you want to check the list of expressions.";

        public string SettingView_UpdateThumbnails = "Update Thumbnails";
        public string SettingView_DefaultSelectedMode = "Default Selected Pattern";
        public string SettingView_ShowHints = "Show Hints";
        public string SettingView_ApplyToAvatar = "Apply to Avatar";
        public string SettingView_Message_ConfirmApplyToAvatar = "Are you sure you want to apply facial expression menu to the avatar?";
        public string SettingView_Message_Succeeded = "Facial expression menu generation is completed!\nThe facial expression menu is merged at the time of upload.";
        public string SettingView_Tooltip_DefaultSelectedMode = "You can change the expression pattern that is selected by default.";

        public string GestureTableView_AddBranch = "Add Expression";
        public string GestureTableView_Separator = " & ";
        public string GestureTableView_ModeIsNotSelected = "Please select an expression pattern in the main window.";

        public string AnimationElement_NewClipName = "NewExpression";
        public string AnimationElement_Message_GuidWasNotFound = "The selected animation clip does not exist in this project.";
        public string AnimationElement_Tooltip_Create = "Create a new animation clip";
        public string AnimationElement_Tooltip_Open = "Open an animation clip";
        public string AnimationElement_Tooltip_Copy = "Copy the attached animation clip";
        public string AnimationElement_Tooltip_Edit = "Open ExpressionEditor and edit the attached animation clip";

        public string ModeNameProvider_NewMode = "NewExpressionPattern";
        public string ModeNameProvider_NewGroup = "NewFolder";
        public string ModeNameProvider_NoExpression = "NoExpression";

        public string InspectorView_TargetAvatarIsNotSpecified = "Please specify the target avatar.";
        public string InspectorView_Launch = "Launch FacialExpressionSwitcher";
        public string InspectorView_TargetAvatar = "Target Avatar";
        public string InspectorView_GenerateModeThumbnails = "Use expression thumbnails as ExpressionMenu icons";
        public string InspectorView_MouthMorphBlendShapes = "MouthMorphBlendShapes";
        public string InspectorView_EmptyBlendShapes = "Press the + button to add blend shapes.";
        public string InspectorView_EmptyObjects = "Press the + button to add an object.";
        public string InspectorView_Message_NotInAvatar = " is not an object in the avatar, so animation is not applied.";
        public string InspectorView_Message_MAVersionError_NotFound = "Failed to retrieve the version of Modular Avatar.";
        public string InspectorView_Message_MAVersionError_1_5_0 = "This tool is not compatible with Modular Avatar 1.5.0. Please use 1.5.1 or higher or 1.4.5.";

        public string InspectorView_Thumbnail = "Expression Thumbnail Setting";
        public string InspectorView_Thumbnail_FOV = "FOV";
        public string InspectorView_Thumbnail_Distance = "Distance";
        public string InspectorView_Thumbnail_HorizontalPosition = "Position (Horizontal)";
        public string InspectorView_Thumbnail_VerticalPosition = "Position (Vertical)";
        public string InspectorView_Thumbnail_HorizontalAngle = "Angle (Horizontal)";
        public string InspectorView_Thumbnail_VerticalAngle = "Angle (Vertical)";
        public string InspectorView_Thumbnail_Reset = "Reset";

        public string InspectorView_AFK = "AFK Face Setting";
        public string InspectorView_AFK_EnterFace = "AFK Enter Face";
        public string InspectorView_AFK_Face = "AFK Face";
        public string InspectorView_AFK_ExitFace = "AFK Exit Face";

        public string InspectorView_ExpressionsMenuSettingItems = "ExpressionMenu Setting Items";
        public string InspectorView_AddConfig_Add = "Add";
        public string InspectorView_AddConfig_Default = "Default Value";
        public string InspectorView_AddConfig_BlinkOff = "Blink OFF";
        public string InspectorView_AddConfig_DanceGimmick = "Dance Gimmick";
        public string InspectorView_AddConfig_ContactLock = "Contact Emote Lock";
        public string InspectorView_AddConfig_Override = "Emote Override";
        public string InspectorView_AddConfig_Voice = "Do Not Transition When Speaking";
        public string InspectorView_AddConfig_HandPattern = "Gesture Setting";
        public string InspectorView_AddConfig_Controller = "Controller Type";

        public string InspectorView_AvatarApplicationSetting = "Avatar Application Setting";
        public string InspectorView_EmoteSelect = "Generate Emote Select Menu";
        public string InspectorView_SmoothAnalogFist = "Smooth Analog Fist";
        public string InspectorView_TransitionDuration = "Transition Duration (sec)";
        public string InspectorView_AddExpressionParameterPrefix = "Add Expression Parameter Prefix (FES_)";
        public string InspectorView_ReplaceBlink = "Replace blink with animation at build time (recommended)";
        public string InspectorView_DisableTrackingControls = "Disable VRCTrackingControls for eyes and mouth at build time (recommended)";

        public string InspectorView_Defaults = "Default Value for Expression Setting";

        public string InspectorView_EditorSetting = "Editor Setting";
        public string InspectorView_AutoSave = "Autosave Scene When Modifying Expression Menu";
        public string InspectorView_GroupDeleteConfirmation = "Confirm When Deleting Folder";
        public string InspectorView_ModeDeleteConfirmation = "Confirm When Deleting Expression Pattern";
        public string InspectorView_BranchDeleteConfirmation = "Confirm When Deleting Expression";

        public string InspectorView_Help = "Help";
        public string InspectorView_Help_Manual = "FacialExpressionSwitcher Manual";

        public string InspectorView_Tooltip_MouthMorphCanceler = "This is used in expressions where Mouth Morph Canceler is enabled. " +
            "During speech, you can prevent disruptions by resetting certain shape key values to their default values (the values set on the scene).";
        public string InspectorView_Tooltip_AddMouthMorphBlendShape = "When selecting blend shapes, you can make multiple selections by \"Ctrl + Click\". " +
            "Also, you can select a range by \"Shift + Click\" or \"Shift + Arrow key\". " +
            "If you make multiple selections or select a range and press \"Add\", you can add blend shapes all at once.";
        public string InspectorView_Tooltip_ConfirmMouthMorphBlendShape = "If Mouth Morph Canceler is not working properly, please make sure that all mouth morph blend shapes have been added. " +
            "If you are morphing the mouth with blend shapes for MMD, do not forget to add those as well.";
        public string InspectorView_Tooltip_AdditionalToggle = "In conjunction with expressions, you can control the ON/OFF of specific objects (such as expression particles). " +
            "Objects specified here can be used in the expression editor.";
        public string InspectorView_Tooltip_AdditionalTransform = "In conjunction with expressions, you can control the position, rotation, and scale of specific objects (such as beast ears and ahoge). " +
            "Objects specified here can be used in the expression editor.";
        public string InspectorView_Tooltip_AFK = "You can set expressions for when you are AFK or for when you start/end AFK.";
        public string InspectorView_Tooltip_Thumbnail = "You can set the camera position and angle for the thumbnails displayed in the ExpressionMenu.";
        public string InspectorView_Tooltip_ExMenu = "You can specify which settings to add to the ExpressionMenu. " +
            "You can reduce the usage of Expression Parameter by reducing the settings.";
        public string InspectorView_Tooltip_ExMenu_EmoteLock = "While this is ON, you can fix the expression.";
        public string InspectorView_Tooltip_ExMenu_BlinkOff = "While this is ON, you can disable blinking.";
        public string InspectorView_Tooltip_ExMenu_Dance = "While this is ON, your expression will move in sync with the dance gimmick. " +
            "Since the FX Layer's functions will be disabled, it will return to the default state if you're using costume switches, etc.";
        public string InspectorView_Tooltip_ExMenu_ContactLock = "While this is ON, you can use the facial fixation function by Contact. " +
            "If you put both hands on your head for 2 seconds, a sound effect will play and your expression will be fixed. " +
            "If you perform the same action when your expression is fixed, the facial fixation will be canceled.";
        public string InspectorView_Tooltip_ExMenu_Override = "While this is ON, you can use the facial overwrite function by Contact. " +
            "Please use it with the FES_EmoteOverrideExample integrated into the avatar (refer to the manual).";
        public string InspectorView_Tooltip_ExMenu_Voice = "While this is ON, your expression will not change during speech. " +
            "If you have an expression with lip-syncing disabled, turning this ON will prevent the mouth from remaining open.";
        public string InspectorView_Tooltip_ExMenu_Gesture = "You can change the settings for the gestures used for specifying expressions.";
        public string InspectorView_Tooltip_ExMenu_Gesture_Swap = "Swap the left and right gestures to change expressions.";
        public string InspectorView_Tooltip_ExMenu_Gesture_DisableLeft = "Disable expression changes by the left-hand gesture (treated as Neutral).";
        public string InspectorView_Tooltip_ExMenu_Gesture_DisableRight = "Disable expression changes by the right-hand gesture (treated as Neutral).";
        public string InspectorView_Tooltip_ExMenu_Controller = "You can disable gestures that are easily input by mistake on each controller.";
        public string InspectorView_Tooltip_ExMenu_Controller_Quest = "Disable expression changes by the 'Open' gesture (treated as Neutral).";
        public string InspectorView_Tooltip_ExMenu_Controller_Index = "Disable expression changes by the 'Fist' gesture (treated as Neutral).";
        public string InspectorView_Tooltip_Application = "You can change the settings when applying the expression menu to the avatar.";
        public string InspectorView_Tooltip_Application_EmoteSelect = "If disabled, the number of expression patterns that can be registered in the Expression Menu is increased by one.";
        public string InspectorView_Tooltip_Defaults = "You can change the settings values right after creating expression patterns or expressions.";
        public string InspectorView_Tooltip_Editor = "You can configure the behavior of this tool's UI. These settings are shared across all projects.";
        public string InspectorView_Tooltip_Help = "You can read the manual for this tool.";

        public string ExpressionEditorView_Rename = "Rename";
        public string ExpressionEditorView_ShowOnlyDifferFromDefaultValue = "Show Only BlendShapes Different From the Scene";
        public string ExpressionEditorView_ReflectInPreviewOnMouseOver = "Reflect In Preview On Mouse Over";
        public string ExpressionEditorView_Delimiter = "Delimiter";
        public string ExpressionEditorView_UncategorizedBlendShapes = "Uncategorized Blend Shapes";
        public string ExpressionEditorView_NoBlendShapes = "No Blend Shapes";
        public string ExpressionEditorView_NoObjects = "No Objects";
        public string ExpressionEditorView_Message_FailedToRename = "Failed to rename. Please enter a different name.";
        public string ExpressionEditorView_Message_BlinkBlendShapeExists = "Blend shapes for blink are included!";
        public string ExpressionEditorView_Message_LipSyncBlendShapeExists = "Blend shapes for lipsync are included!";
        public string ExpressionEditorView_Tooltip_Delimiter = "By entering the delimiter for the blend shapes, you can display the blend shapes by category. " +
            "For example, if the category name is \"=====Eye=====\", please enter \"=====\".";

        public string FESBackupper_Message_FailedToFindTargetAvatar = "Failed to find tareget avatar. Make sure the avatar is active.";
        public string FESBackupper_Message_FailedToFindToggleObject = "Failed to find toggle object.";
        public string FESBackupper_Message_FailedToFindTransformObject = "Failed to find transform object.";

        public string FxGenerator_Message_FaceMeshNotFound = "Face mesh was not found. Default face and blink may not be properly animated.";
        public string FxGenerator_Message_BlinkBlendShapeNotFound = "Blink blend shape was not found. Blink may not be properly animated.";

        public string ErrorHandler_Message_ErrorOccured = "An error has occurred. Please check the console.";

        public string ExMenu_EmoteSelect = "Emote Select";
        public string ExMenu_Setting = "Setting";
        public string ExMenu_EmoteLock = "Emote Lock ON [NOT Saved]";
        public string ExMenu_BlinkOff = "Blink OFF [NOT Saved]";
        public string ExMenu_DanceGimmick = "Enable Dance Gimmick [NOT Saved]";
        public string ExMenu_ContactLock = "Enable Contact Emote Lock";
        public string ExMenu_Override = "Enable Emote Override";
        public string ExMenu_Voice = "Do Not Transition When Speaking";
        public string ExMenu_HandPattern = "Gesture Setting";
        public string ExMenu_HandPattern_SwapLR = "Swap LR";
        public string ExMenu_HandPattern_DisableLeft = "Disable Left";
        public string ExMenu_HandPattern_DisableRight = "Disable Right";
        public string ExMenu_Controller = "Controller";
        public string ExMenu_Controller_Quest = "Quest Controller";
        public string ExMenu_Controller_Index = "Index Controller";

        public string Hints_AddMode = "Create an expression pattern and set the correspondence between expressions and gestures. " +
                                       "If there are multiple expression patterns, you can switch them in the ExpressionMenu.";
        public string Hints_SelectMode = "When you select an expression pattern, you can set the correspondence between expressions and gestures on the right screen.";
        public string Hints_RegisteredFreeSpace1 = "In the expression menu, you can register up to <0> expression patterns or folders. " +
                                                   "If you want to register more than <1> expression patterns, please create a folder.";
        public string Hints_RegisteredFreeSpace0 = "You can no longer register any more expression patterns or folders in the expression menu. " +
                                                   "Please move some expression patterns to another folder or archive to free up space.";
        public string Hints_RegisteredFreeSpaceMinus = "The number of expression patterns and folders registered in the expression menu has exceeded the limit. " +
                                                       "Please move the expression patterns to a different folder or archive.";
        public string Hints_GroupFreeSpace1 = "You can register up to 8 expression patterns or folders in a folder.";
        public string Hints_GroupFreeSpace0 = "You cannot register any more expression patterns or folders in this folder. " +
                                              "Please move some expression patterns to another folder or archive to free up space.";
        public string Hints_Archive = "Archived expression patterns are not added to the avatar. " +
                                      "If you want to use an expression pattern, please drag it to the \"Expression Menu\" on the left screen.";
        public string Hints_AddExpression = "You can set the correspondence between expressions and gestures for the currently selected expression pattern.";
        public string Hints_Simplified = "Turning off the simplified display will show settings for each expression and thumbnails.";
        public string Hints_AnimationMenu = "When you hover the mouse cursor over the thumbnail of an expression, a menu is displayed, and you can create or edit expression animations.";
        public string Hints_ExpressionPriority = "If there are multiple expressions corresponding to the same gesture, the expression at the top is prioritized. " +
                                                 "You can rearrange the order of expressions by drag & drop.";
        public string Hints_BlinkBlendShapeIncluded = "If the expression contains blend shapes for blinking, there's a possibility that your expression may be overwritten by the blinking animation and not play correctly.";
        public string Hints_LipSyncBlendShapeIncluded = "If your expression contains blend shapes for lip-syncing, there's a possibility that the lip-sync may not function correctly.";
        public string Hints_ExpressionPreview = "The expression preview screen can be moved, rotated, and zoomed in and out in the same way as the scene view. " +
            "The camera settings (like FOV) of the expression preview screen inherit the settings from the scene view.";
        public string Hints_ControlWidthWheel = "The sliders for the shape key values can also be controlled with the mouse wheel.";
    }
}
