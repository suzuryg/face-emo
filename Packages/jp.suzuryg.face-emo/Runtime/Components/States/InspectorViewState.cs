﻿using UnityEngine;

namespace Suzuryg.FaceEmo.Components.States
{
    public class InspectorViewState : ScriptableObject
    {
        public bool IsApplyingToMultipleAvatarsOpened = false;
        public bool IsMouthMorphBlendShapesOpened = false;
        public bool IsAddtionalSkinnedMeshesOpened = false;
        public bool IsAddtionalToggleOpened = false;
        public bool IsAddtionalTransformOpened = false;
        public bool IsAFKOpened = false;
        public bool IsThumbnailOpened = false;
        public bool IsExpressionsMenuItemsOpened = false;
        public bool IsAvatarApplicationOpened = false;
        public bool IsDefaultsOpened = false;
        public bool IsEditorSettingOpened = false;
    }
}
