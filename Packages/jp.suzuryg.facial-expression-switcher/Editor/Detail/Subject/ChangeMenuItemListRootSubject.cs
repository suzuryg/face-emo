using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Subject
{
    public class ChangeMenuItemListRootSubject
    {
        public IObservable<string> Observable => _subject.AsObservable().Synchronize();

        private Subject<string> _subject = new Subject<string>();

        public void OnNext(string rootGroupId) => _subject.OnNext(rootGroupId);
    }
}
