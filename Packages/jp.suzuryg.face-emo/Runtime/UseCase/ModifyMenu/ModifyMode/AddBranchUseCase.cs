using Suzuryg.FaceEmo.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FaceEmo.UseCase.ModifyMenu.ModifyMode
{
    public interface IAddBranchUseCase
    {
        void Handle(string menuId, string modeId, IEnumerable<Condition> conditions = null, int? order = null, DefaultsProvider defaultsProvider = null);
    }

    public interface IAddBranchPresenter
    {
        IObservable<(AddBranchResult addBranchResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(AddBranchResult addBranchResult, in IMenu menu, string errorMessage = "");
    }

    public enum AddBranchResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidDestination,
        ArgumentNull,
        Error,
    }

    public class AddBranchPresenter : IAddBranchPresenter
    {
        public IObservable<(AddBranchResult, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(AddBranchResult, IMenu, string)> _subject = new Subject<(AddBranchResult, IMenu, string)>();

        public void Complete(AddBranchResult addBranchResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((addBranchResult, menu, errorMessage));
        }
    }

    public class AddBranchUseCase : IAddBranchUseCase
    {
        IMenuRepository _menuRepository;
        UpdateMenuSubject _updateMenuSubject;
        IAddBranchPresenter _addBranchPresenter;

        public AddBranchUseCase(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, IAddBranchPresenter addBranchPresenter)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _addBranchPresenter = addBranchPresenter;
        }

        public void Handle(string menuId, string modeId, IEnumerable<Condition> conditions = null, int? order = null, DefaultsProvider defaultsProvider = null)
        {
            try
            {
                if (menuId is null || modeId is null)
                {
                    _addBranchPresenter.Complete(AddBranchResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _addBranchPresenter.Complete(AddBranchResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanAddBranchTo(modeId))
                {
                    _addBranchPresenter.Complete(AddBranchResult.InvalidDestination, menu);
                    return;
                }

                menu.AddBranch(modeId, conditions, defaultsProvider);

                if (order.HasValue)
                {
                    var mode = menu.GetMode(modeId);
                    menu.ChangeBranchOrder(modeId, from: mode.Branches.Count - 1, to: order.Value);
                }

                _menuRepository.Save(menuId, menu, "AddBranch");
                _addBranchPresenter.Complete(AddBranchResult.Succeeded, menu);
                _updateMenuSubject.OnNext(menu);
            }
            catch (Exception ex)
            {
                _addBranchPresenter.Complete(AddBranchResult.Error, null, ex.ToString());
            }
        }
    }
}
