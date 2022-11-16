using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Data
{
    public class SerializableAnimation : ScriptableObject
    {
        public string GUID;

        public void Save(Domain.Animation animation)
        {
            GUID = animation.GUID;
        }

        public Domain.Animation Load()
        {
            return new Domain.Animation(GUID);
        }
    }
}
