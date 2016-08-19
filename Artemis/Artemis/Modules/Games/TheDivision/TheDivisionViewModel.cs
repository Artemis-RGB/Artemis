using Artemis.InjectionFactories;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Games.TheDivision
{
    public sealed class TheDivisionViewModel : GameViewModel
    {
        public TheDivisionViewModel(MainManager main, IProfileEditorVmFactory pFactory, TheDivisionModel model)
            : base(main, model, pFactory)
        {
            DisplayName = "The Division";
        }
    }
}