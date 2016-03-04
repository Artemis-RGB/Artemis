using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.AmbientLightning
{
    internal class AmbientLightningEffectViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public AmbientLightningEffectViewModel(MainManager mainManager)
        {
            // Subscribe to main model
            MainManager = mainManager;
            MainManager.Events.Subscribe(this);

            // Settings are loaded from file by class
            EffectSettings = new AmbientLightningEffectSettings();

            // Create effect model and add it to MainManager
            EffectModel = new AmbientLightningEffectModel(mainManager, (AmbientLightningEffectSettings) EffectSettings);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }


        public static string Name => "Ambient Lightning";

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}