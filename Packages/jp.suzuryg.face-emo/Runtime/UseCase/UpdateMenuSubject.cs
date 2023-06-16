using Suzuryg.FaceEmo.Domain;
using System;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase
{
    public class UpdateMenuSubject
    {
        public IObservable<IMenu> Observable => _subject.AsObservable().Synchronize();

        private Subject<IMenu> _subject = new Subject<IMenu>();

        public void OnNext(IMenu menu) => _subject.OnNext(menu);
    }
}
