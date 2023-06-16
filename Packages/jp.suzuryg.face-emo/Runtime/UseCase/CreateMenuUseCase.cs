using Suzuryg.FaceEmo.Domain;
using System;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase
{
    public interface ICreateMenuUseCase
    {
        void Handle(string menuId);
    }

    public interface ICreateMenuPresenter
    {
        IObservable<(CreateMenuResult createMenuResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(CreateMenuResult createMenuResult, in IMenu menu, string errorMessage = "");
    }

    public enum CreateMenuResult
    {
        Succeeded,
        ArgumentNull,
        Error,
    }

    public class CreateMenuPresenter : ICreateMenuPresenter
    {
        public IObservable<(CreateMenuResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(CreateMenuResult, IMenu, string)> _subject = new Subject<(CreateMenuResult, IMenu, string)>();

        public void Complete(CreateMenuResult createMenuResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((createMenuResult, menu, errorMessage));
        }
    }

    public class CreateMenuUseCase : ICreateMenuUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        ICreateMenuPresenter _createMenuPresenter;

        public CreateMenuUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, ICreateMenuPresenter createMenuPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _createMenuPresenter = createMenuPresenter;
        }

        public void Handle(string menuId)
        {
            try
            {
                if (menuId is null)
                {
                    _createMenuPresenter.Complete(CreateMenuResult.ArgumentNull, null);
                    return;
                }

                var menu = new Menu();
                _menuRepository.Save(menuId, menu, "CreateMenu");
                _createMenuPresenter.Complete(CreateMenuResult.Succeeded, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _createMenuPresenter.Complete(CreateMenuResult.Error, null, ex.ToString());
            }
        }
    }
}
