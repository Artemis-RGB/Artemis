using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Games.ProjectCars
{
    public sealed class ProjectCarsViewModel : GameViewModel
    {
        public ProjectCarsViewModel(MainManager main, IKernel kernel,
            [Named("ProjectCarsModel")] GameModel model) : base(main, model, kernel)
        {
            DisplayName = "Project CARS";
        }
    }
}