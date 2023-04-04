using Suzuryg.FacialExpressionSwitcher.Domain;
using System;
using UniRx;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface IModifyMenuPropertiesUseCase
    {
        void Handle(
            string menuId,
            Avatar avatar = null,
            string defaultSelection = null);
    }

    public interface IModifyMenuPropertiesPresenter
    {
        IObservable<(ModifyMenuPropertiesResult modifyMenuPropertiesResult, IMenu menu, string errorMessage)> Observable { get; }

        void Complete(ModifyMenuPropertiesResult modifyMenuPropertiesResult, in IMenu menu, string errorMessage = "");
    }

    public enum ModifyMenuPropertiesResult
    {
        Succeeded,
        MenuDoesNotExist,
        ModeIsNotContained,
        ArgumentNull,
        Error,
    }

    public class ModifyMenuPropertiesPresenter : IModifyMenuPropertiesPresenter
    {
        public IObservable<(ModifyMenuPropertiesResult modifyMenuPropertiesResult, IMenu menu, string errorMessage)> Observable => _subject.AsObservable();

        private Subject<(ModifyMenuPropertiesResult modifyMenuPropertiesResult, IMenu menu, string errorMessage)> _subject = new Subject<(ModifyMenuPropertiesResult modifyMenuPropertiesResult, IMenu menu, string errorMessage)>();

        public void Complete(ModifyMenuPropertiesResult modifyModePropertiesResult, in IMenu menu, string errorMessage = "")
        {
            _subject.OnNext((modifyModePropertiesResult, menu, errorMessage));
        }
    }

    public class ModifyMenuPropertiesUseCase : IModifyMenuPropertiesUseCase
    {
        IMenuRepository _menuRepository;
        IModifyMenuPropertiesPresenter _modifyMenuPropertiesPresenter;

        public ModifyMenuPropertiesUseCase(IMenuRepository menuRepository, IModifyMenuPropertiesPresenter modifyModePropertiesPresenter)
        {
            _menuRepository = menuRepository;
            _modifyMenuPropertiesPresenter = modifyModePropertiesPresenter;
        }

        public void Handle(
            string menuId,
            Avatar avatar = null,
            string defaultSelection = null)
        {
            try
            {
                if (menuId is null)
                {
                    _modifyMenuPropertiesPresenter.Complete(ModifyMenuPropertiesResult.ArgumentNull, null);
                    return;
                }

                if (!_menuRepository.Exists(menuId))
                {
                    _modifyMenuPropertiesPresenter.Complete(ModifyMenuPropertiesResult.MenuDoesNotExist, null);
                    return;
                }

                var menu = _menuRepository.Load(menuId);

                menu.Avatar = avatar ?? menu.Avatar;
                
                if (defaultSelection is string)
                {
                    if (menu.CanSetDefaultSelectionTo(defaultSelection))
                    {
                        menu.SetDefaultSelection(defaultSelection);
                    }
                    else
                    {
                        _modifyMenuPropertiesPresenter.Complete(ModifyMenuPropertiesResult.ModeIsNotContained, null);
                        return;
                    }
                }

                _menuRepository.Save(menuId, menu, "ModifyMenuProperties");
                _modifyMenuPropertiesPresenter.Complete(ModifyMenuPropertiesResult.Succeeded, menu);
            }
            catch (Exception ex)
            {
                _modifyMenuPropertiesPresenter.Complete(ModifyMenuPropertiesResult.Error, null, ex.ToString());
            }
        }
    }
}
