using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using System.Collections.Generic;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IMergeExistingMenuUseCase
    {
        void SetPresenter(IMergeExistingMenuPresenter mergeExistingMenuPresenter);
        void Handle(IReadOnlyList<IExistingMenuItem> existingMenuItems);
    }

    public interface IMergeExistingMenuPresenter
    {
        void Complete(MergeExistingMenuResult mergeExistingMenuResult, MergedMenuItemList mergedMenuItems, in Menu menu, string errorMessage = "");
    }

    public enum MergeExistingMenuResult
    {
        Succeeded,
        MenuIsNotOpened,
        ArgumentNull,
        InvalidArgument,
        Error,
    }

    public class MergeExistingMenuUseCase : IMergeExistingMenuUseCase
    {
        IMergeExistingMenuPresenter _mergeExistingMenuPresenter;
        MenuEditingSession _menuEditingSession;

        public MergeExistingMenuUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(IMergeExistingMenuPresenter mergeExistingMenuPresenter)
        {
            _mergeExistingMenuPresenter = mergeExistingMenuPresenter;
        }

        public void Handle(IReadOnlyList<IExistingMenuItem> existingMenuItems)
        {
            try
            {
                if (existingMenuItems is null)
                {
                    _mergeExistingMenuPresenter?.Complete(MergeExistingMenuResult.ArgumentNull, null, null);
                    return;
                }

                if (!_menuEditingSession.IsOpened)
                {
                    _mergeExistingMenuPresenter?.Complete(MergeExistingMenuResult.MenuIsNotOpened, null, null);
                    return;
                }

                var menu = _menuEditingSession.Menu;

                if (!menu.CanGetMergedMenu(existingMenuItems))
                {
                    _mergeExistingMenuPresenter?.Complete(MergeExistingMenuResult.InvalidArgument, null, menu);
                    return;
                }

                _mergeExistingMenuPresenter?.Complete(MergeExistingMenuResult.Succeeded, menu.GetMergedMenu(existingMenuItems), menu);
            }
            catch (Exception ex)
            {
                _mergeExistingMenuPresenter?.Complete(MergeExistingMenuResult.Error, null, null, ex.ToString());
            }
        }
    }
}
