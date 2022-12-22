using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface IMoveMenuItemUseCase
    {
        void Handle(string menuId, string source, string destination, int? index = null);
        void Handle(string menuId, IReadOnlyList<string> sources, string destination, int? index = null);
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
        ArgumentNullOrEmpty,
        Error,
    }

    public class MoveMenuItemPresenter : IMoveMenuItemPresenter
    {
        public IObservable<(MoveMenuItemResult moveMenuItemResult, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(MoveMenuItemResult moveMenuItemResult, IMenu menu, string errorMessage)> _subject = new Subject<(MoveMenuItemResult moveMenuItemResult, IMenu menu, string errorMessage)>();

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
            Handle(menuId, new List<string>() { source }, destination, index);
        }

        public void Handle(string menuId, IReadOnlyList<string> sources, string destination, int? index = null)
        {
            try
            {
                if (menuId is null || sources is null || sources.Contains(null) || sources.Count == 0 || destination is null)
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.ArgumentNullOrEmpty, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanMoveMenuItemFrom(sources))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.InvalidSource, menu);
                    return;
                }

                if (!menu.CanMoveMenuItemTo(sources, destination))
                {
                    _moveMenuItemPresenter.Complete(MoveMenuItemResult.InvalidDestination, menu);
                    return;
                }

                menu.MoveMenuItem(sources, destination, index);
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
