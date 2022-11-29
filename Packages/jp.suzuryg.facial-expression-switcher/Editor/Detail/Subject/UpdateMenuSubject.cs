using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.Detail.Subject
{
    public class UpdateMenuSubject
    {
        public IObservable<IMenu> Observable => _subject.AsObservable().Synchronize();

        private Subject<IMenu> _subject = new Subject<IMenu>();

        public void OnNext(IMenu menu) => _subject.OnNext(menu);
    }
}
