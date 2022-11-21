using Suzuryg.FacialExpressionSwitcher.Detail.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    // TODO: Add usecase
    public class SerializableSetting : ScriptableObject
    {
        // TODO: Null reference handling
        public LocalizationDictionary LocalizationDictionary;
    }
}
