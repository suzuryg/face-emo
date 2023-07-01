using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public interface ICopyBranchUseCase
    {
        void Handle(string menuId, string modeId, int branchIndex, Animation baseAnimation, Animation leftAnimation, Animation rightAnimation, Animation bothAnimation);
    }

    public interface ICopyBranchPresenter
    {
        IObservable<(CopyBranchResult copyBranchResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(CopyBranchResult copyBranchResult, in IMenu menu, string errorMessage = "");
    }

    public enum CopyBranchResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class CopyBranchPresenter : ICopyBranchPresenter
    {
        public IObservable<(CopyBranchResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(CopyBranchResult, IMenu, string)> _subject = new Subject<(CopyBranchResult, IMenu, string)>();

        public void Complete(CopyBranchResult CopyBranchResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((CopyBranchResult, menu, errorMessage));
        }
    }

    public class CopyBranchUseCase : ICopyBranchUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        ICopyBranchPresenter _copyBranchPresenter;

        public CopyBranchUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, ICopyBranchPresenter copyBranchPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _copyBranchPresenter = copyBranchPresenter;
        }

        public void Handle(string menuId, string modeId, int branchIndex, Animation baseAnimation, Animation leftAnimation, Animation rightAnimation, Animation bothAnimation)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _copyBranchPresenter.Complete(CopyBranchResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _copyBranchPresenter.Complete(CopyBranchResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanAddBranchTo(modeId))
                {
                    _copyBranchPresenter.Complete(CopyBranchResult.InvalidDestination, menu);
                    return;
                }

                menu.CopyBranch(modeId, branchIndex, modeId);

                var mode = menu.GetMode(modeId);
                menu.ChangeBranchOrder(modeId, from: mode.Branches.Count - 1, to: branchIndex);

                menu.SetAnimation(baseAnimation, modeId, branchIndex, BranchAnimationType.Base);
                menu.SetAnimation(leftAnimation, modeId, branchIndex, BranchAnimationType.Left);
                menu.SetAnimation(rightAnimation, modeId, branchIndex, BranchAnimationType.Right);
                menu.SetAnimation(bothAnimation, modeId, branchIndex, BranchAnimationType.Both);

                _menuRepository.Save(menuId, menu, "CopyBranch");
                _updateMenuSubject.OnNext(menu);

                _copyBranchPresenter.Complete(CopyBranchResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _copyBranchPresenter.Complete(CopyBranchResult.Error, null, ex.ToString());
            }
        }
    }
}
