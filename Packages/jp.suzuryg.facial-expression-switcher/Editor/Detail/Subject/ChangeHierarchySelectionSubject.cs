using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Subject
{
    public class ChangeHierarchySelectionSubject
    {
        public IObservable<IReadOnlyList<string>> Observable => _subject.AsObservable().Synchronize();

        private Subject<IReadOnlyList<string>> _subject = new Subject<IReadOnlyList<string>>();

        public void OnNext(IReadOnlyList<string> selectedMenuItemIds) => _subject.OnNext(selectedMenuItemIds);
    }
}
