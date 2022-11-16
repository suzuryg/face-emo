using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class MockAnimationEditor : IAnimationEditor
    {
        private HashSet<string> _idSet = new HashSet<string>();

        public Animation Create(string path)
        {
            var animation = new Animation(GetNewId());
            return animation;
        }

        public void Open(Animation animation)
        {
            // NOP
        }

        private string GetNewId()
        {
            var id = Guid.NewGuid().ToString("N");
            while (_idSet.Contains(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
            _idSet.Add(id);
            return id;
        }
    }
}
