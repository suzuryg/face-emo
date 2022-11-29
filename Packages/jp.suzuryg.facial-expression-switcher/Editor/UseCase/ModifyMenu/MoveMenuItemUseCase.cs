using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IMoveMenuItemUseCase
    {
        void Handle(string menuId, string source, string destination, int? index = null);
    }

    public interface IMoveMenuItemPresenter
    {
        IObservable<(MoveMenuItemResult moveMenuItemResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(MoveMenuItemResult moveMenuItemResult, in IMenu menu, string errorMessage = "");
    }

    public enum MoveMenuItemResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidSource,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class MoveMenuItemPresenter : IMoveMenuItemPresenter
    {
        public IObservable<(MoveMenuItemResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(MoveMenuItemResult, IMenu, string)> _subject = new Subject<(MoveMenuItemResult, IMenu, string)>();

        public void Complete(MoveMenuItemResult moveMenuItemResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((moveMenuItemResult, menu, errorMessage));
        }
    }

    public class MoveMenuItemUseCase : IMoveMenuItemUseCase
    {
        IMenuRepository _menuRepository;
        IMoveMenuItemPresenter _moveMenuItemPresenter;

        public MoveMenuItemUseCase(IMenuRepository menuRepository, IMoveMenuItemPresenter moveMenuItemPresenter)
        {
            _menuRepository = menuRepository;
            _moveMenuItemPresenter = moveMenuItemPresenter;
        }

        public void Handle(string menuId, string source, string destination, int? index = null)
        {
            try
            {
                if (menuId is null || source is null || destination is null)
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanMoveMenuItemFrom(source))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.InvalidSource, menu);
                    return;
                }

                if (!menu.CanMoveMenuItemTo(source, destination))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.InvalidDestination, menu);
                    return;
                }

                menu.MoveMenuItem(source, destination, index);
                _menuRepository.Save(menuId, menu, "MoveMenuItem");
                _moveMenuItemPresenter.Complete(MoveMenuItemResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _moveMenuItemPresenter.Complete(MoveMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
