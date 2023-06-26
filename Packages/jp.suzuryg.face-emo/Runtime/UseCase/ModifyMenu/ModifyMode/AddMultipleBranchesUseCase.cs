using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public interface IAddMultipleBranchesUseCase
    {
        void Handle(string menuId, string modeId, IReadOnlyList<IEnumerable<Condition>> branches, int? order = null);
    }

    public interface IAddMultipleBranchesPresenter
    {
        IObservable<(AddMultipleBranchesResult addMultipleBranchesResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(AddMultipleBranchesResult addMultipleBranchesResult, in IMenu menu, string errorMessage = "");
    }

    public enum AddMultipleBranchesResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class AddMultipleBranchesPresenter : IAddMultipleBranchesPresenter
    {
        public IObservable<(AddMultipleBranchesResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(AddMultipleBranchesResult, IMenu, string)> _subject = new Subject<(AddMultipleBranchesResult, IMenu, string)>();

        public void Complete(AddMultipleBranchesResult AddMultipleBranchesResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((AddMultipleBranchesResult, menu, errorMessage));
        }
    }

    public class AddMultipleBranchesUseCase : IAddMultipleBranchesUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IAddMultipleBranchesPresenter _addMultipleBranchesPresenter;

        public AddMultipleBranchesUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IAddMultipleBranchesPresenter addMultipleBranchesPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _addMultipleBranchesPresenter = addMultipleBranchesPresenter;
        }

        public void Handle(string menuId, string modeId, IReadOnlyList<IEnumerable<Condition>> branches, int? order = null)
        {
            try
            {
                if (menuId is null || modeId is null || branches is null)
                {
                    _addMultipleBranchesPresenter.Complete(AddMultipleBranchesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _addMultipleBranchesPresenter.Complete(AddMultipleBranchesResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanAddBranchTo(modeId))
                {
                    _addMultipleBranchesPresenter.Complete(AddMultipleBranchesResult.InvalidDestination, menu);
                    return;
                }

                var mode = menu.GetMode(modeId);

                // Reverse order processing to insert a series of branches at specific locations.
                var specifyOrder = order.HasValue && order.Value < mode.Branches.Count;
                var sortedBranches = specifyOrder ? branches.Reverse() : branches;

                foreach (var conditions in sortedBranches)
                {
                    menu.AddBranch(modeId, conditions);

                    if (specifyOrder)
                    {
                        menu.ChangeBranchOrder(modeId, from: mode.Branches.Count - 1, to: order.Value);
                    }
                }

                _menuRepository.Save(menuId, menu, "AddMultipleBranches");
                _addMultipleBranchesPresenter.Complete(AddMultipleBranchesResult.Succeeded, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _addMultipleBranchesPresenter.Complete(AddMultipleBranchesResult.Error, null, ex.ToString());
            }
        }
    }
}
