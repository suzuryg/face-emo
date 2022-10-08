using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IApplyMenuUseCase
    {
        void SetPresenter(IApplyMenuPresenter applyMenuPresenter);
        void Handle(MergedMenuItemList mergedMenuItems);
    }

    public interface IApplyMenuPresenter
    {
        void Complete(ApplyMenuResult applyMenuResult, in Menu menu, string errorMessage = "");
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
        MenuIsNotOpened,
        InvalidArgument,
        ArgumentNull,
        Error,
    }

    public class ApplyMenuUseCase : IApplyMenuUseCase
    {
        IApplyMenuPresenter _applyMenuPresenter;
        MenuEditingSession _menuEditingSession;
        IMenuApplier _menuApplier;

        public ApplyMenuUseCase(MenuEditingSession menuEditingSession, IMenuApplier menuApplier)
        {
            _menuEditingSession = menuEditingSession;
            _menuApplier = menuApplier;
        }

        public void SetPresenter(IApplyMenuPresenter applyMenuPresenter)
        {
            _applyMenuPresenter = applyMenuPresenter;
        }

        public void Handle(MergedMenuItemList mergedMenuItems)
        {
            try
            {
                if (mergedMenuItems is null)
                {
                    _applyMenuPresenter?.Complete(ApplyMenuResult.ArgumentNull, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _applyMenuPresenter?.Complete(ApplyMenuResult.MenuIsNotOpened, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanUpdateOrderAndInsertIndices(mergedMenuItems))
                {
                    _applyMenuPresenter?.Complete(ApplyMenuResult.InvalidArgument, menu);
                    return;
                }

                //if (!AreMenuItemsContained(menu, mergedMenuItems))
                //{
                //    _applyMenuPresenter?.Complete(ApplyMenuResult.MenuItemsAreNotContained, menu);
                //    return;
                //}

                menu.UpdateOrderAndInsertIndices(mergedMenuItems);
                _menuEditingSession.SetAsModified();
                _menuApplier.Apply(mergedMenuItems, menu);
                _applyMenuPresenter?.Complete(ApplyMenuResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _applyMenuPresenter?.Complete(ApplyMenuResult.Error, null, ex.ToString());
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
