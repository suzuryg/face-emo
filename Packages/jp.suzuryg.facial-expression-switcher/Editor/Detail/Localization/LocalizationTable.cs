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
        public string Common_Message_DeleteBranch = "Are you sure you want to delete the currently selected expression?";
        public string Common_Message_ClearMouthMorphBlendShapes = "Are you sure you want to clear mouth morph blend shapes?";
        public string Common_Message_NotPossibleInPlayMode = "This operation is not possible during Play mode.";
        public string Common_Message_GuidWasNotFound = "The selected asset does not exist in this project.";

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

        public string SettingView_UpdateThumbnails = "Update Thumbnails";
        public string SettingView_DefaultSelectedMode = "Default Selected Pattern";
        public string SettingView_ApplyToAvatar = "Apply to Avatar";
        public string SettingView_Message_ConfirmApplyToAvatar = "Are you sure you want to apply facial expression menu to the avatar?";
        public string SettingView_Message_Succeeded = "Facial expression menu generation is completed!\nThe facial expression menu is merged at the time of upload.";

        public string GestureTableView_AddBranch = "Add Expression";

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
        public string InspectorView_GenerateModeThumbnails = "Use expression thumbnails as ExpressionsMenu icons";
        public string InspectorView_MouthMorphBlendShapes = "MouthMorphBlendShapes";
        public string InspectorView_Message_NotInAvatar = " is not an object in the avatar, so animation is not applied.";
        public string InspectorView_Message_MAVersionError_NotFound = "Failed to retrieve the version of Modular Avatar.";
        public string InspectorView_Message_MAVersionError_1_5_0 = "This tool is not compatible with Modular Avatar 1.5.0. Please use 1.5.1 or higher or 1.4.5.";

        public string InspectorView_Thumbnail = "Expression Thumbnail Setting";
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

        public string InspectorView_ExpressionsMenuSettingItems = "Expressions Menu Setting Items";
        public string InspectorView_AddConfig_EmoteLock = "Emote Lock (1bit)";
        public string InspectorView_AddConfig_BlinkOff = "Blink OFF (1bit)";
        public string InspectorView_AddConfig_DanceGimmick = "Dance Gimmick (1bit)";
        public string InspectorView_AddConfig_ContactLock = "Contact Emote Lock (1bit)";
        public string InspectorView_AddConfig_Override = "Emote Override (1bit)";
        public string InspectorView_AddConfig_Voice = "Do Not Transition When Speaking (1bit)";
        public string InspectorView_AddConfig_HandPattern = "Gesture Setting (3bit)";
        public string InspectorView_AddConfig_Controller = "Controller Type (2bit)";

        public string InspectorView_AvatarApplicationSetting = "Avatar Application Setting";
        public string InspectorView_SmoothAnalogFist = "Smooth Analog Fist";
        public string InspectorView_TransitionDuration = "Transition Duration (sec)";
        public string InspectorView_AddExpressionParameterPrefix = "Add Expression Parameter Prefix (FES_)";
        public string InspectorView_ReplaceBlink = "Replace blink with animation at build time (recommended)";
        public string InspectorView_DisableTrackingControls = "Disable VRCTrackingControls for eyes and mouth at build time (recommended)";

        public string InspectorView_EditorSetting = "Editor Setting";
        public string InspectorView_ShowHints = "Show Hints";
        public string InspectorView_AutoSave = "Autosave Scene When Modifying Expression Menu";
        public string InspectorView_GroupDeleteConfirmation = "Confirm When Deleting Folder";
        public string InspectorView_ModeDeleteConfirmation = "Confirm When Deleting Expression Pattern";
        public string InspectorView_BranchDeleteConfirmation = "Confirm When Deleting Expression";

        public string InspectorView_Help = "Help";
        public string InspectorView_Help_Manual = "FacialExpressionSwitcher Manual";

        public string ExpressionEditorView_ShowOnlyDifferFromDefaultValue = "Show Only BlendShapes Different From Default";
        public string ExpressionEditorView_ReflectInPreviewOnMouseOver = "Reflect In Preview On Mouse Over";
        public string ExpressionEditorView_Delimiter = "Delimiter";
        public string ExpressionEditorView_UncategorizedBlendShapes = "Uncategorized Blend Shapes";
        public string ExpressionEditorView_NoBlendShapes = "No Blend Shapes";
        public string ExpressionEditorView_NoObjects = "No Objects";
        public string ExpressionEditorView_Message_BlinkBlendShapeExists = "Blend shapes for blink are included!";
        public string ExpressionEditorView_Message_LipSyncBlendShapeExists = "Blend shapes for lipsync are included!";

        public string FESBackupper_Message_FailedToFindTargetAvatar = "Failed to find tareget avatar. Make sure the avatar is active.";
        public string FESBackupper_Message_FailedToFindToggleObject = "Failed to find toggle object.";
        public string FESBackupper_Message_FailedToFindTransformObject = "Failed to find transform object.";

        public string FxGenerator_Message_FaceMeshNotFound = "Face mesh was not found. Default face and blink may not be properly animated.";
        public string FxGenerator_Message_BlinkBlendShapeNotFound = "Blink blend shape was not found. Blink may not be properly animated.";

        public string ErrorHandler_Message_ErrorOccured = "An error has occurred. Please check the console.";

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
                                       "If there are multiple expression patterns, you can switch them in the ExpressionsMenu.";
        public string Hints_SelectMode = "When you select an expression pattern, you can set the correspondence between expressions and gestures on the right screen.";
        public string Hints_RegisteredFreeSpace1 = "In the expression menu, you can register up to 7 expression patterns or folders. " +
                                                   "If you want to register more than 8 expression patterns, please create a folder.";
        public string Hints_RegisteredFreeSpace0 = "You can no longer register any more expression patterns or folders in the expression menu. " +
                                                   "Please move some expression patterns to another folder or archive to free up space.";
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
    }
}
