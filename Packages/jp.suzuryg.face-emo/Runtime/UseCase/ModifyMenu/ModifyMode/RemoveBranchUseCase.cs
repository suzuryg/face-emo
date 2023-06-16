using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public interface IRemoveBranchUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex);
    }

    public interface IRemoveBranchPresenter
    {
        IObservable<(RemoveBranchResult removeBranchResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(RemoveBranchResult removeBranchResult, in IMenu menu, string errorMessage = "");
    }

    public enum RemoveBranchResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidBranch,
        ArgumentNull,
        Error,
    }

    public class RemoveBranchPresenter : IRemoveBranchPresenter
    {
        public IObservable<(RemoveBranchResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(RemoveBranchResult, IMenu, string)> _subject = new Subject<(RemoveBranchResult, IMenu, string)>();

        public void Complete(RemoveBranchResult removeBranchResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((removeBranchResult, menu, errorMessage));
        }
    }

    public class RemoveBranchUseCase : IRemoveBranchUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IRemoveBranchPresenter _removeBranchPresenter;

        public RemoveBranchUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IRemoveBranchPresenter removeBranchPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _removeBranchPresenter = removeBranchPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _removeBranchPresenter.Complete(RemoveBranchResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _removeBranchPresenter.Complete(RemoveBranchResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanRemoveBranch(modeId, branchIndex))
                {
                    _removeBranchPresenter.Complete(RemoveBranchResult.InvalidBranch, menu);
                    return;
                }

                menu.RemoveBranch(modeId, branchIndex);

                _menuRepository.Save(menuId, menu, "RemoveBranch");
                _removeBranchPresenter.Complete(RemoveBranchResult.Succeeded, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _removeBranchPresenter.Complete(RemoveBranchResult.Error, null, ex.ToString());
            }
        }
    }
}
