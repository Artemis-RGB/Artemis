using Ninject;
using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.TheDivision
{
    public sealed class TheDivisionViewModel : GameViewModel
    {
        public TheDivisionViewModel(MainManager main, IKernel kernel, [Named("TheDivisionModel")] GameModel model)
            : base(main, model, kernel)
        {
            DisplayName = "The Division";
        }
    }
}