using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class MockCreateMenuPresenter : ICreateMenuPresenter
    {
        public CreateMenuResult Result { get; private set; }

        public event System.Action<CreateMenuResult, IMenu, string> OnCompleted;

        void ICreateMenuPresenter.Complete(CreateMenuResult createMenuResult, in IMenu menu, string errorMessage)
        {
            Result = createMenuResult;
        }
    }

    public class CreateMenuUseCaseTests
    {
        [Test]
        public void Test()
        {
            UseCaseTestsInstaller useCaseTestsInstaller = new UseCaseTestsInstaller();
            useCaseTestsInstaller.Install();

            var menuRepository = useCaseTestsInstaller.Container.Resolve<IMenuRepository>();
            var menuId = UseCaseTestSetting.MenuId;

            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            MockCreateMenuPresenter mockCreateMenuPresenter = useCaseTestsInstaller.Container.Resolve<ICreateMenuPresenter>() as MockCreateMenuPresenter;

            // null
            createMenuUseCase.Handle(null);
            Assert.That(mockCreateMenuPresenter.Result, Is.EqualTo(CreateMenuResult.ArgumentNull));

            // Create new
            createMenuUseCase.Handle(menuId);
            Assert.That(mockCreateMenuPresenter.Result, Is.EqualTo(CreateMenuResult.Succeeded));
            Assert.That(menuRepository.Load(menuId), Is.Not.Null);

            // Overwrite
            createMenuUseCase.Handle(menuId);
            Assert.That(mockCreateMenuPresenter.Result, Is.EqualTo(CreateMenuResult.Succeeded));
            Assert.That(menuRepository.Load(menuId), Is.Not.Null);
        }
    }
}
