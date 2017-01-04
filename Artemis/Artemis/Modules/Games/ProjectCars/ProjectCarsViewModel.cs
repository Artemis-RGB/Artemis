using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.ProjectCars
{
    public sealed class ProjectCarsViewModel : ModuleViewModel
    {
        public ProjectCarsViewModel(MainManager mainManager, [Named(nameof(ProjectCarsModel))] ModuleModel moduleModel,
            IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "Project CARS";
        }

        public override bool UsesProfileEditor => true;
    }
}