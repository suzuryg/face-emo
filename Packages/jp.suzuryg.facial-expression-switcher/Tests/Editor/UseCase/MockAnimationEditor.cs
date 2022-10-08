using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class MockAnimationEditor : IAnimationEditor
    {
        private HashSet<string> _idSet = new HashSet<string>();

        public IAnimation Create(string path)
        {
            var animation = new MockAnimation();
            animation.GUID = GetNewId();
            return animation;
        }

        public void Open(IAnimation animation)
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
