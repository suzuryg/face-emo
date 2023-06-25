using Suzuryg.FaceEmo.Domain;
using Suzuryg.FaceEmo.UseCase;
using System;
using System.Collections.Generic;
using System.Threading;
using UniRx;

namespace Suzuryg.FaceEmo.Detail.View
{
    public class SelectionSynchronizer : IDisposable
    {
        public IObservable<ViewSelection> OnSynchronizeSelection => _onSynchronizeSelection.AsObservable();

        private BehaviorSubject<ViewSelection> _onSynchronizeSelection;

        private IMenuRepository _menuRepository;
        private UpdateMenuSubject _updateMenuSubject;
        private ViewSelection _viewSelection;

        private IMenu _menu;

        private object _lockSelection = new object();

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SelectionSynchronizer(IMenuRepository menuRepository, UpdateMenuSubject updateMenuSubject, ViewSelection viewSelection)
        {
            _menuRepository = menuRepository;
            _updateMenuSubject = updateMenuSubject;
            _viewSelection = viewSelection;

            _onSynchronizeSelection = new BehaviorSubject<ViewSelection>(_viewSelection);

            // Update menu event handler
            _updateMenuSubject.Observable.Synchronize().Subscribe(x => OnMenuUpdated(x.menu)).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnMenuUpdated(IMenu menu) => _menu = menu;

        private IMenu GetMenu()
        {
            if (_menu is null)
            {
                _menu = _menuRepository.Load(string.Empty);
            }
            return _menu;
        }

        public void ChangeHierarchyViewSelection(string menuItemId)
        {
            // Skip processing if the lock object cannot be obtained.
            var isEntered = false;
            try
            {
                isEntered = Monitor.TryEnter(_lockSelection);
                if (isEntered)
                {
                    _viewSelection.HierarchyView = menuItemId;

                    var menu = GetMenu();

                    // If the selection target is MenuItemList.
                    if (menuItemId == Menu.RegisteredId || menuItemId == Menu.UnregisteredId || menu.ContainsGroup(menuItemId))
                    {
                        // Clear other view selections.
                        _viewSelection.MenuItemListView = null;
                        _viewSelection.BranchListView = -1;
                        _viewSelection.GestureTableView = null;
                    }
                    // If the selection target is Mode.
                    else if (menu.ContainsMode(menuItemId))
                    {
                        // If the selection of MenuItemListView is changed, initialize the selection of other views.
                        if (_viewSelection.MenuItemListView != menuItemId)
                        {
                            _viewSelection.MenuItemListView = menuItemId;
                            _viewSelection.BranchListView = 0;
                            _viewSelection.GestureTableView = null;
                        }
                        // If the selection of MenuItemListView is not changed, maintain the selection of the other Views.
                        else
                        {
                            // NOP
                        }
                    }
                    // If the selection target is incorrect.
                    else
                    {
                        // Clear other view selections.
                        _viewSelection.MenuItemListView = null;
                        _viewSelection.BranchListView = -1;
                        _viewSelection.GestureTableView = null;
                    }

                    // Synchronize selections.
                    _onSynchronizeSelection.OnNext(_viewSelection);
                }
            }
            finally
            {
                if (isEntered) { Monitor.Exit(_lockSelection); }
            }
        }

        public void ChangeMenuItemListViewSelection(string menuItemId)
        {
            // Skip processing if the lock object cannot be obtained.
            var isEntered = false;
            try
            {
                isEntered = Monitor.TryEnter(_lockSelection);
                if (isEntered)
                {
                    var previousMenuItemId = _viewSelection.MenuItemListView;
                    _viewSelection.MenuItemListView = menuItemId;

                    var menu = GetMenu();

                    // If the selection target is MenuItemList.
                    if (menuItemId == Menu.RegisteredId || menuItemId == Menu.UnregisteredId || menu.ContainsGroup(menuItemId))
                    {
                        // Clear other view selections.
                        _viewSelection.BranchListView = -1;
                        _viewSelection.GestureTableView = null;
                    }
                    // If the selection target is Mode.
                    else if (menu.ContainsMode(menuItemId))
                    {
                        // If the selection of MenuItemListView is changed, initialize the selection of other views.
                        if (previousMenuItemId != menuItemId)
                        {
                            _viewSelection.BranchListView = 0;
                            _viewSelection.GestureTableView = null;
                        }
                        // If the selection of MenuItemListView is not changed, maintain the selection of the other Views.
                        else
                        {
                            // NOP
                        }
                    }
                    // If the selection target is incorrect.
                    else
                    {
                        // Clear other view selections.
                        _viewSelection.BranchListView = -1;
                        _viewSelection.GestureTableView = null;
                    }

                    // Synchronize selections.
                    _onSynchronizeSelection.OnNext(_viewSelection);
                }
            }
            finally
            {
                if (isEntered) { Monitor.Exit(_lockSelection); }
            }
        }

        public void ChangeBranchListViewSelection(int branchIndex)
        {
            // Skip processing if the lock object cannot be obtained.
            var isEntered = false;
            try
            {
                isEntered = Monitor.TryEnter(_lockSelection);
                if (isEntered)
                {
                    _viewSelection.BranchListView = branchIndex;
                    _viewSelection.GestureTableView = null;

                    // Synchronize selections.
                    _onSynchronizeSelection.OnNext(_viewSelection);
                }
            }
            finally
            {
                if (isEntered) { Monitor.Exit(_lockSelection); }
            }
        }

        public void ChangeGestureTableViewSelection(HandGesture left, HandGesture right)
        {
            // Skip processing if the lock object cannot be obtained.
            var isEntered = false;
            try
            {
                isEntered = Monitor.TryEnter(_lockSelection);
                if (isEntered)
                {
                    _viewSelection.GestureTableView = (left, right);

                    var menu = GetMenu();

                    // If the branch containing the selected gesture exists, select the branch.
                    _viewSelection.BranchListView = -1;
                    var id = _viewSelection.MenuItemListView;
                    if (menu.ContainsMode(id))
                    {
                        var mode = menu.GetMode(id);
                        var branch = mode.GetGestureCell(left, right);
                        if (branch is IBranch)
                        {
                            for (int i = 0; i < mode.Branches.Count; i++)
                            {
                                if (ReferenceEquals(branch, mode.Branches[i]))
                                {
                                    _viewSelection.BranchListView = i;
                                    break;
                                }
                            }
                        }
                    }

                    // Synchronize selections.
                    _onSynchronizeSelection.OnNext(_viewSelection);
                }
            }
            finally
            {
                if (isEntered) { Monitor.Exit(_lockSelection); }
            }
        }
    }
}
