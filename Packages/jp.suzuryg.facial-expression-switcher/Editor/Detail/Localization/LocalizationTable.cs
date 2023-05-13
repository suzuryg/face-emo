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
        public string Common_Message_InvalidDestination = "You cannot move to here. Check the number of modes and groups.";
        public string Common_Message_DeleteGroup = "Are you sure you want to delete the following group?";
        public string Common_Message_DeleteMode = "Are you sure you want to delete the following mode?";
        public string Common_Message_DeleteBranch = "Are you sure you want to delete the currently selected branch?";
        public string Common_Message_ClearMouthMorphBlendShapes = "Are you sure you want to clear mouth morph blend shapes?";
        public string Common_Message_NotPossibleInPlayMode = "This operation is not possible during Play mode.";
        public string Common_Message_GuidWasNotFound = "The selected asset does not exist in this project.";

        public string HierarchyView_Title = "HierarchyView";
        public string HierarchyView_RegisteredMenuItemList = "ItemsToRegister";
        public string HierarchyView_UnregisteredMenuItemList = "ItemsNotToRegister";
        public string HierarchyView_Message_CanNotRename = "The 'Use animation name as mode name' setting is enabled. Disable the setting if you want to change the mode name.";
        public string HierarchyView_Tooltip_AddMode = "Create a new expression mode in the selected hierarchy";
        public string HierarchyView_Tooltip_AddGroup = "Create a new expression group in the selected hierarchy";
        public string HierarchyView_Tooltip_Copy = "Copy the selected expression mode or expression group in the same hierarchy";
        public string HierarchyView_Tooltip_Delete = "Delete the selected expression mode or expression group";

        public string MenuItemListView_Title = "MenuItemView";
        public string MenuItemListView_UseAnimationNameAsDisplayName = "Use Animation Name As Mode Name";
        public string MenuItemListView_Tooltip_AddMode = "Create a new expression mode in the current hierarchy";
        public string MenuItemListView_Tooltip_AddGroup = "Create a new expression group in the current hierarchy";
        public string MenuItemListView_Tooltip_Copy = "Copy the selected expression mode or expression group in the same hierarchy";
        public string MenuItemListView_Tooltip_Delete = "Delete the selected expression mode or expression group";

        public string MenuItemListView_EyeTracking = "Enable Eye Tracking";
        public string MenuItemListView_MouthTracking = "Enable Mouth Tracking";
        public string MenuItemListView_Blink = "Enable Blink";
        public string MenuItemListView_MouthMorphCanceler = "Enable Mouth Morph Canceler";
        public string MenuItemListView_Empty = "This group is empty.";

        public string BranchListView_Title = "BranchView";
        public string BranchListView_OpenGestureTable = "Open Gesture Table";
        public string BranchListView_UseLeftTrigger = "Use Left Trigger";
        public string BranchListView_UseRightTrigger = "Use Right Trigger";
        public string BranchListView_NotReachableBranch = "This branch is not used.";
        public string BranchListView_LeftTriggerAnimation = "Left Trigger";
        public string BranchListView_RightTriggerAnimation = "Right Trigger";
        public string BranchListView_BothTriggersAnimation = "Both Triggers";
        public string BranchListView_EmptyBranch = "There are no branches.";
        public string BranchListView_EmptyCondition = "There are no conditions.";
        public string BranchListView_Condition = "Conditions";
        public string BranchListView_Left = "LeftHand";
        public string BranchListView_Right = "RightHand";
        public string BranchListView_OneSide = "OneHand";
        public string BranchListView_Either = "EitherHands";
        public string BranchListView_Both = "BothHands";
        public string BranchListView_Equals = "Equals";
        public string BranchListView_NotEqual = "NotEqual";

        public string SettingView_UpdateThumbnails = "Update Thumbnails";
        public string SettingView_DefaultSelectedMode = "Default Selected Mode";
        public string SettingView_ApplyToAvatar = "Apply to Avatar";
        public string SettingView_Message_ConfirmApplyToAvatar = "Are you sure you want to apply facial expression menu to the avatar?";
        public string SettingView_Message_Succeeded = "Facial expression menu generation is completed!";

        public string GestureTableView_AddBranch = "Add Branch";

        public string AnimationElement_Message_GuidWasNotFound = "The selected animation clip does not exist in this project.";
        public string AnimationElement_Tooltip_Create = "Create a new animation clip";
        public string AnimationElement_Tooltip_Open = "Open an animation clip";
        public string AnimationElement_Tooltip_Copy = "Copy the attached animation clip";
        public string AnimationElement_Tooltip_Edit = "Open ExpressionEditor and edit the attached animation clip";

        public string ModeNameProvider_NoExpression = "NoExpression";

        public string InspectorView_Launch = "Launch FacialExpressionSwitcher";
        public string InspectorView_TargetAvatar = "Target Avatar";
        public string InspectorView_GenerateModeThumbnails = "Generate Expression Mode Thumbnails";
        public string InspectorView_MouthMorphBlendShapes = "MouthMorphBlendShapes";
        public string InspectorView_Message_NotInAvatar = " is not an object in the avatar, so animation is not applied.";

        public string InspectorView_Thumbnail = "Thumbnail Setting";
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
        public string InspectorView_AddConfig_HandPattern = "Hand Pattern (3bit)";
        public string InspectorView_AddConfig_Controller = "Controller Type (2bit)";

        public string InspectorView_AvatarApplicationSetting = "Avatar Application Setting";
        public string InspectorView_SmoothAnalogFist = "Smooth Analog Fist";
        public string InspectorView_TransitionDuration = "Transition Duration (sec)";
        public string InspectorView_AddExpressionParameterPrefix = "Add Expression Parameter Prefix (FES_)";
        public string InspectorView_ReplaceBlink = "Replace blink with animation at build time (recommended)";
        public string InspectorView_DisableTrackingControls = "Disable VRCTrackingControls for eyes and mouth at build time (recommended)";

        public string InspectorView_EditorSetting = "Editor Setting";
        public string InspectorView_GroupDeleteConfirmation = "Confirm When Deleting Group";
        public string InspectorView_ModeDeleteConfirmation = "Confirm When Deleting Mode";
        public string InspectorView_BranchDeleteConfirmation = "Confirm When Deleting Branch";

        public string ExpressionEditorView_ShowOnlyDifferFromDefaultValue = "Show Only Differ From Default Value";
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
        public string ExMenu_HandPattern = "Hand Pattern";
        public string ExMenu_HandPattern_SwapLR = "Swap LR";
        public string ExMenu_HandPattern_DisableLeft = "Disable Left";
        public string ExMenu_HandPattern_DisableRight = "Disable Right";
        public string ExMenu_Controller = "Controller";
        public string ExMenu_Controller_Quest = "Quest Controller";
        public string ExMenu_Controller_Index = "Index Controller";
    }
}
