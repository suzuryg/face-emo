using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IMergeExistingMenuUseCase
    {
        void Handle(string menuId, IReadOnlyList<IExistingMenuItem> existingMenuItems);
    }

    public interface IMergeExistingMenuPresenter
    {
        IObservable<(MergeExistingMenuResult, MergedMenuItemList, IMenu, string)> Observable { get; }

        void Complete(MergeExistingMenuResult mergeExistingMenuResult, MergedMenuItemList mergedMenuItems, in IMenu menu, string errorMessage = "");
    }

    public enum MergeExistingMenuResult
    {
        Succeeded,
        MenuDoesNotExist,
        ArgumentNull,
        InvalidArgument,
        Error,
    }

    public class MergeExistingMenuPresenter : IMergeExistingMenuPresenter
    {
        public IObservable<(MergeExistingMenuResult, MergedMenuItemList, IMenu, string)> Observable => _subject.AsObservable().Synchronize();

        private Subject<(MergeExistingMenuResult, MergedMenuItemList, IMenu, string)> _subject = new Subject<(MergeExistingMenuResult, MergedMenuItemList, IMenu, string)>();

        public void Complete(MergeExistingMenuResult mergeExistingMenuResult, MergedMenuItemList mergedMenuItems, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((mergeExistingMenuResult, mergedMenuItems, menu, errorMessage));
        }
    }

    public class MergeExistingMenuUseCase : IMergeExistingMenuUseCase
    {
        IMenuRepository _menuRepository;
        IMergeExistingMenuPresenter _mergeExistingMenuPresenter;

        public MergeExistingMenuUseCase(IMenuRepository menuRepository, IMergeExistingMenuPresenter mergeExistingMenuPresenter)
        {
            _menuRepository = menuRepository;
            _mergeExistingMenuPresenter = mergeExistingMenuPresenter;
        }

        public void Handle(string menuId, IReadOnlyList<IExistingMenuItem> existingMenuItems)
        {
            try
            {
                if (menuId is null || existingMenuItems is null)
                {
                    _mergeExistingMenuPresenter.Complete(MergeExistingMenuResult.ArgumentNull, null, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _mergeExistingMenuPresenter.Complete(MergeExistingMenuResult.MenuDoesNotExist, null, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanGetMergedMenu(existingMenuItems))
                {
                    _mergeExistingMenuPresenter.Complete(MergeExistingMenuResult.InvalidArgument, null, menu);
                    return;
                }

                _mergeExistingMenuPresenter.Complete(MergeExistingMenuResult.Succeeded, menu.GetMergedMenu(existingMenuItems), menu);
            }
            catch (Exception ex)
            {
                _mergeExistingMenuPresenter.Complete(MergeExistingMenuResult.Error, null, null, ex.ToString());
            }
        }
    }
}
