using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IApplyMenuUseCase
    {
        void Handle(string menuId, MergedMenuItemList mergedMenuItems);
    }

    public interface IApplyMenuPresenter
    {
        event Action<ApplyMenuResult, IMenu, string> OnCompleted;

        void Complete(ApplyMenuResult applyMenuResult, in IMenu menu, string errorMessage = "");
    }

    public interface IMenuApplier
    {
        // Presenterに渡す値を返す？ ← ApplyMenuResultと名前が被る
        // menuのデフォルト選択が含まれていない場合はエラーを出す
        void Apply(MergedMenuItemList mergedMenuItems, Menu menu);
    }

    public enum ApplyMenuResult
    {
        Succeeded,
        MenuDoesNotExist,
        InvalidArgument,
        ArgumentNull,
        Error,
    }

    public class ApplyMenuPresenter : IApplyMenuPresenter
    {
        public event Action<ApplyMenuResult, IMenu, string> OnCompleted;

        public void Complete(ApplyMenuResult applyMenuResult, in IMenu menu, string errorMessage = "")
        {
            OnCompleted(applyMenuResult, menu, errorMessage);
        }
    }

    public class ApplyMenuUseCase : IApplyMenuUseCase
    {
        IMenuRepository _menuRepository;
        IMenuApplier _menuApplier;
        IApplyMenuPresenter _applyMenuPresenter;

        public ApplyMenuUseCase(IMenuRepository menuRepository, IMenuApplier menuApplier, IApplyMenuPresenter applyMenuPresenter)
        {
            _menuRepository = menuRepository;
            _menuApplier = menuApplier;
            _applyMenuPresenter = applyMenuPresenter;
        }

        public void Handle(string menuId, MergedMenuItemList mergedMenuItems)
        {
            try
            {
                if (menuId is null || mergedMenuItems is null)
                {
                    _applyMenuPresenter.Complete(ApplyMenuResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _applyMenuPresenter.Complete(ApplyMenuResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                if (!menu.CanUpdateOrderAndInsertIndices(mergedMenuItems))
                {
                    _applyMenuPresenter.Complete(ApplyMenuResult.InvalidArgument, menu);
                    return;
                }

                //if (!AreMenuItemsContained(menu, mergedMenuItems))
                //{
                //    _applyMenuPresenter.Complete(ApplyMenuResult.MenuItemsAreNotContained, menu);
                //    return;
                //}

                menu.UpdateOrderAndInsertIndices(mergedMenuItems);

                _menuRepository.Save(menuId, menu);

                _menuApplier.Apply(mergedMenuItems, menu);
                _applyMenuPresenter.Complete(ApplyMenuResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _applyMenuPresenter.Complete(ApplyMenuResult.Error, null, ex.ToString());
            }
        }

        //private bool AreMenuItemsContained(Menu menu, List<IMergedMenuItem> mergedMenuItems)
        //{
        //    foreach (var item in mergedMenuItems)
        //    {
        //        if (item is IMenuItem menuItem && !menu.Contains(menuItem.Id))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
    }
}
