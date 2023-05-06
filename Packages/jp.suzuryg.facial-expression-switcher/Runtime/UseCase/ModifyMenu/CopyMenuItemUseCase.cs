using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase.ModifyMenu
{
    public interface ICopyMenuItemUseCase
    {
        void Handle(string menuItemId, string displayName);
    }

    public interface ICopyMenuItemPresenter
    {
        IObservable<(CopyMenuItemResult copyMenuItemResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(CopyMenuItemResult copyMenuItemResult,in IMenu menu, string errorMessage = "");
    }

    public enum CopyMenuItemResult
    {
        Succeeded,
        ArgumentNull,
        InvalidSource,
        InvalidDestination,
        Error,
    }

    public class CopyMenuItemPresenter : ICopyMenuItemPresenter
    {
        public IObservable<(CopyMenuItemResult copyMenuItemResult, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(CopyMenuItemResult copyMenuItemResult, IMenu menu, string errorMessage)> _subject = new Subject<(CopyMenuItemResult copyMenuItemResult, IMenu menu, string errorMessage)>();

        public void Complete(CopyMenuItemResult copyMenuItemResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((copyMenuItemResult, menu, errorMessage));
        }
    }

    public class CopyMenuItemUseCase : ICopyMenuItemUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        ICopyMenuItemPresenter _copyMenuItemPresenter;

        public CopyMenuItemUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, ICopyMenuItemPresenter copyMenuItemPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _copyMenuItemPresenter = copyMenuItemPresenter;
        }

        public void Handle(string menuItemId, string displayName)
        {
            try
            {
                if (string.IsNullOrEmpty(menuItemId) || string.IsNullOrEmpty(displayName))
                {
                    _copyMenuItemPresenter.Complete(CopyMenuItemResult.ArgumentNull, null);
                    return;
                }

                var menu = _menuRepository.Load(string.Empty);
                if (menu.ContainsMode(menuItemId))
                {
                    var mode = menu.GetMode(menuItemId);
                    var parentId = mode.Parent.GetId();

                    if (!menu.CanAddModeTo(parentId))
                    {
                        _copyMenuItemPresenter.Complete(CopyMenuItemResult.InvalidDestination, null);
                        return;
                    }

                    var copied = menu.CopyMode(menuItemId, parentId);
                    menu.ModifyModeProperties(copied, displayName: displayName);
                }
                else if (menu.ContainsGroup(menuItemId))
                {
                    var group = menu.GetGroup(menuItemId);
                    var parentId = group.Parent.GetId();

                    if (!menu.CanAddModeTo(parentId))
                    {
                        _copyMenuItemPresenter.Complete(CopyMenuItemResult.InvalidDestination, null);
                        return;
                    }

                    var copied = menu.CopyGroup(menuItemId, parentId);
                    menu.ModifyGroupProperties(copied, displayName: displayName);
                }
                else
                {
                    _copyMenuItemPresenter.Complete(CopyMenuItemResult.InvalidSource, null);
                    return;
                }

                _menuRepository.Save(string.Empty, menu, "CopyMenuItem");
                _copyMenuItemPresenter.Complete(CopyMenuItemResult.Succeeded, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _copyMenuItemPresenter.Complete(CopyMenuItemResult.Error, null, ex.ToString());
            }
        }
    }
}
