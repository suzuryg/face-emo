using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Localization
{
    public class LocalizationTable : ScriptableObject
    {
        public string Common_Delete = "Delete";
        public string Common_Cancel = "Cancel";
        public string Common_Message_DeleteGroup = "Are you sure you want to delete the following group?";
        public string Common_Message_DeleteMode = "Are you sure you want to delete the following mode?";
        public string Common_Message_DeleteBranch = "Are you sure you want to delete the currently selected branch?";

        public string MainView_UpdateThumbnails = "Update Thumbnails";

        public string HierarchyView_Title = "HierarchyView";
        public string HierarchyView_RegisteredMenuItemList = "ItemsToRegister";
        public string HierarchyView_UnregisteredMenuItemList = "ItemsNotToRegister";
        public string HierarchyView_Message_CanNotRename = "The 'Use animation name as mode name' setting is enabled. Disable the setting if you want to change the mode name.";

        public string MenuItemListView_Title = "MenuItemView";
        public string MenuItemListView_UseAnimationNameAsDisplayName = "Use Animation Name As Mode Name";

        public string MenuItemListView_EyeTracking = "Enable Eye Tracking";
        public string MenuItemListView_MouthTracking = "Enable Mouth Tracking";
        public string MenuItemListView_Blink = "Enable Blink";
        public string MenuItemListView_MouthMorphCanceler = "Enable Mouth Morph Canceler";
        public string MenuItemListView_Empty = "This group is empty.";

        public string BranchListView_Title = "BranchView";
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
        public string BranchListView_Neutral = "Neutral";
        public string BranchListView_Fist = "Fist";
        public string BranchListView_HandOpen = "HandOpen";
        public string BranchListView_Fingerpoint = "Fingerpoint";
        public string BranchListView_Victory = "Victory";
        public string BranchListView_RockNRoll = "RockNRoll";
        public string BranchListView_HandGun = "HandGun";
        public string BranchListView_ThumbsUp = "ThumbsUp";
        public string BranchListView_Equals = "Equals";
        public string BranchListView_NotEqual = "NotEqual";

        public string GestureTableView_AddBranch = "Add Branch";
        public string GestureTableView_Neutral = "Neutral";
        public string GestureTableView_Fist = "Fist";
        public string GestureTableView_HandOpen = "HandOpen";
        public string GestureTableView_Fingerpoint = "Fingerpoint";
        public string GestureTableView_Victory = "Victory";
        public string GestureTableView_RockNRoll = "RockNRoll";
        public string GestureTableView_HandGun = "HandGun";
        public string GestureTableView_ThumbsUp = "ThumbsUp";

        public string SettingView_GroupDeleteConfirmation = "Confirm When Deleting Group";
        public string SettingView_ModeDeleteConfirmation = "Confirm When Deleting Mode";
        public string SettingView_BranchDeleteConfirmation = "Confirm When Deleting Branch";

        public string AnimationElement_Message_GuidWasNotFound = "The selected animation clip does not exist in this project.";

        public string ModeNameProvider_NoExpression = "NoExpression";

        public string InspectorView_TargetAvatar = "Target Avatar";
        public string InspectorView_AddtionalToggleObjects = "Addtional Expression Objects (Toggle)";
        public string InspectorView_AddtionalTransformObjects = "Addtional Expression Objects (Transform)";

        public string InspectorView_AddConfig_EmoteLock = "Emote Lock (1bit)";
        public string InspectorView_AddConfig_BlinkOff = "Blink OFF (1bit)";
        public string InspectorView_AddConfig_DanceGimmick = "Dance Gimmick (1bit)";
        public string InspectorView_AddConfig_ContactLock = "Contact Emote Lock (1bit)";
        public string InspectorView_AddConfig_Override = "Emote Override (1bit)";
        public string InspectorView_AddConfig_HandPriority = "Hand Priority (4bit)";
        public string InspectorView_AddConfig_Controller = "Controller Type (2bit)";

        public string ExMenu_Setting = "Setting";
        public string ExMenu_EmoteLock = "Emote Lock ON [NOT Saved]";
        public string ExMenu_BlinkOff = "Blink OFF [NOT Saved]";
        public string ExMenu_DanceGimmick = "Enable Dance Gimmick [NOT Saved]";
        public string ExMenu_ContactLock = "Enable Contact Emote Lock";
        public string ExMenu_Override = "Enable Emote Override";
        public string ExMenu_HandPriority = "Hand Priority";
        public string ExMenu_HandPriority_PrimeLeft = "Prime Left";
        public string ExMenu_HandPriority_PrimeRight = "Prime Right";
        public string ExMenu_HandPriority_OnlyLeft = "Only Left";
        public string ExMenu_HandPriority_OnlyRight = "Only Right";
        public string ExMenu_Controller = "Controller";
        public string ExMenu_Controller_Quest = "Quest Controller";
        public string ExMenu_Controller_Index = "Index Controller";
    }
}
