using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Subject
{
    public class ChangeBranchSelectionSubject
    {
        public IObservable<int> Observable => _subject.AsObservable().Synchronize();

        private Subject<int> _subject = new Subject<int>();

        public void OnNext(int branchIndex) => _subject.OnNext(branchIndex);
    }
}
