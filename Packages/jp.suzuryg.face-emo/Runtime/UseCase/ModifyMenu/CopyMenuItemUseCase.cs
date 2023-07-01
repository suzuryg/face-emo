using Suzuryg.FaceEmo.Domain;
using System;
using System.Linq;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu
{
    public interface ICopyMenuItemUseCase
    {
        void Handle(string menuItemId, string displayName);
    }

    public interface ICopyMenuItemPresenter
    {
        IObservable<(CopyMenuItemResult copyMenuItemResult, string copiedItemId, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(CopyMenuItemResult copyMenuItemResult, string copiedItemId, in IMenu menu, string errorMessage = "");
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
        public IObservable<(CopyMenuItemResult copyMenuItemResult, string copiedItemId, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(CopyMenuItemResult copyMenuItemResult, string copiedItemId, IMenu menu, string errorMessage)> _subject = new Subject<(CopyMenuItemResult copyMenuItemResult, string copiedItemId, IMenu menu, string errorMessage)>();

        public void Complete(CopyMenuItemResult copyMenuItemResult, string copiedItemId, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((copyMenuItemResult, copiedItemId, menu, errorMessage));
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
                    _copyMenuItemPresenter.Complete(CopyMenuItemResult.ArgumentNull, null, null);
                    return;
                }

                var menu = _menuRepository.Load(string.Empty);
                string copiedItemId = null;

                if (menu.ContainsMode(menuItemId))
                {
                    var mode = menu.GetMode(menuItemId);
                    var parentId = mode.Parent.GetId();

                    if (!menu.CanAddMenuItemTo(parentId))
                    {
                        _copyMenuItemPresenter.Complete(CopyMenuItemResult.InvalidDestination, null, null);
                        return;
                    }

                    copiedItemId = menu.CopyMode(menuItemId, parentId);
                    menu.ModifyModeProperties(copiedItemId, displayName: displayName);

                    // Insert after the copy source
                    menu.MoveMenuItem(new[] { copiedItemId }, parentId, mode.Parent.Order.ToList().FindIndex(x => x == menuItemId) + 1);
                }
                else if (menu.ContainsGroup(menuItemId))
                {
                    var group = menu.GetGroup(menuItemId);
                    var parentId = group.Parent.GetId();

                    if (!menu.CanAddMenuItemTo(parentId))
                    {
                        _copyMenuItemPresenter.Complete(CopyMenuItemResult.InvalidDestination, null, null);
                        return;
                    }

                    copiedItemId = menu.CopyGroup(menuItemId, parentId);
                    menu.ModifyGroupProperties(copiedItemId, displayName: displayName);

                    // Insert after the copy source
                    menu.MoveMenuItem(new[] { copiedItemId }, parentId, group.Parent.Order.ToList().FindIndex(x => x == menuItemId) + 1);
                }
                else
                {
                    _copyMenuItemPresenter.Complete(CopyMenuItemResult.InvalidSource, null, null);
                    return;
                }

                _menuRepository.Save(string.Empty, menu, "CopyMenuItem");
                _updateMenuSubject.OnNext(menu);

                _copyMenuItemPresenter.Complete(CopyMenuItemResult.Succeeded, copiedItemId, menu);
            }
            catch (Exception ex)
            {
                _copyMenuItemPresenter.Complete(CopyMenuItemResult.Error, null, null, ex.ToString());
            }
        }
    }
}
