using Suzuryg.FacialExpressionSwitcher.Domain;
using System;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public interface ICreateMenuUseCase
    {
        void SetPresenter(ICreateMenuPresenter createMenuPresenter);
        void Handle();
    }

    public interface ICreateMenuPresenter
    {
        void Complete(CreateMenuResult createMenuResult, in Menu menu, string errorMessage = "");
    }

    public enum CreateMenuResult
    {
        Succeeded,
        MenuIsNotSaved,
        Error,
    }

    public class CreateMenuUseCase : ICreateMenuUseCase
    {
        ICreateMenuPresenter _createMenuPresenter;
        MenuEditingSession _menuEditingSession;

        public CreateMenuUseCase(MenuEditingSession menuEditingSession)
        {
            _menuEditingSession = menuEditingSession;
        }

        public void SetPresenter(ICreateMenuPresenter createMenuPresenter)
        {
            _createMenuPresenter = createMenuPresenter;
        }

        public void Handle()
        {
            try
            {
                if (_menuEditingSession.Create())
                {
                    _createMenuPresenter?.Complete(CreateMenuResult.Succeeded, _menuEditingSession.Menu);
                }
                else
                {
                    _createMenuPresenter?.Complete(CreateMenuResult.MenuIsNotSaved, _menuEditingSession.Menu);
                }
            }
            catch (Exception ex)
            {
                _createMenuPresenter?.Complete(CreateMenuResult.Error, null, ex.ToString());
            }
        }
    }
}
