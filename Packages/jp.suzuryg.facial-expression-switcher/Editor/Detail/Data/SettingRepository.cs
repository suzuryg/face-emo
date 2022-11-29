using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    // TODO: Is interface needed?
    public class SettingRepository
    {
        private static readonly string SettingPath = $"Assets/Suzuryg/{DomainConstants.SystemName}/{DomainConstants.SystemName}Setting.asset";

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        // TODO: Error handling
        public SerializableSetting Load()
        {
            return AssetDatabase.LoadAssetAtPath<SerializableSetting>(SettingPath);
        }

        public void Save()
        //public void Save(Setting setting)
        {
            throw new NotImplementedException();
        }
    }
}
