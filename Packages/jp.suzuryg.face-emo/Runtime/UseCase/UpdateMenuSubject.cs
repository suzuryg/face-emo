using Suzuryg.FaceEmo.Domain;
using System;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase
{
    public class UpdateMenuSubject
    {
        public IObservable<(IMenu menu, bool isModified)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(IMenu menu, bool isModified)> _subject = new Subject<(IMenu menu, bool isModified)>();

        public void OnNext(IMenu menu, bool isModified = true) => _subject.OnNext((menu, isModified));
    }
}
