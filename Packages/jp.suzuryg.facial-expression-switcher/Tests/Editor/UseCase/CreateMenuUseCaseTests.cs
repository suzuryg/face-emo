using Suzuryg.FacialExpressionSwitcher.Domain;
using NUnit.Framework;

namespace Suzuryg.FacialExpressionSwitcher.UseCase
{
    public class MockCreateMenuPresenter : ICreateMenuPresenter
    {
        public CreateMenuResult Result { get; private set; }

        void ICreateMenuPresenter.Complete(CreateMenuResult createMenuResult, in Menu menu, string errorMessage)
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

            // Before creation
            MenuEditingSession menuEditingSession = useCaseTestsInstaller.Container.Resolve<MenuEditingSession>();
            Assert.That(menuEditingSession.Menu, Is.Null);
            Assert.That(menuEditingSession.IsModified, Is.False);

            // Create new
            CreateMenuUseCase createMenuUseCase = useCaseTestsInstaller.Container.Resolve<CreateMenuUseCase>();
            MockCreateMenuPresenter mockCreateMenuPresenter = new MockCreateMenuPresenter();
            createMenuUseCase.SetPresenter(mockCreateMenuPresenter);

            createMenuUseCase.Handle();
            Assert.That(mockCreateMenuPresenter.Result, Is.EqualTo(CreateMenuResult.Succeeded));
            Assert.That(menuEditingSession.Menu, Is.Not.Null);
            Assert.That(menuEditingSession.IsModified, Is.True);

            // Re-create
            createMenuUseCase.Handle();
            Assert.That(mockCreateMenuPresenter.Result, Is.EqualTo(CreateMenuResult.MenuIsNotSaved));
            Assert.That(menuEditingSession.Menu, Is.Not.Null);
            Assert.That(menuEditingSession.IsModified, Is.True);

            // Save & re-create
            menuEditingSession.SaveAs("destination");
            Assert.That(menuEditingSession.Menu, Is.Not.Null);
            Assert.That(menuEditingSession.IsModified, Is.False);
            createMenuUseCase.Handle();
            Assert.That(mockCreateMenuPresenter.Result, Is.EqualTo(CreateMenuResult.Succeeded));
            Assert.That(menuEditingSession.Menu, Is.Not.Null);
            Assert.That(menuEditingSession.IsModified, Is.True);
        }
    }
}
