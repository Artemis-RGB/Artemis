using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.TypeHole
{
    public class TypeHoleViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public TypeHoleViewModel(MainManager mainManager)
        {
            // Subscribe to main model
            MainManager = mainManager;
            MainManager.Events.Subscribe(this);

            // Create effect model and add it to MainManager
            EffectModel = new TypeHoleModel(mainManager);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public static string Name => "Type Holes (NYI)";

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}