using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.TypeWave
{
    public class TypeWaveViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public TypeWaveViewModel(MainManager mainManager)
        {
            // Subscribe to main model
            MainManager = mainManager;
            MainManager.Events.Subscribe(this);

            // Settings are loaded from file by class
            EffectSettings = new TypeWaveSettings();

            // Create effect model and add it to MainManager
            EffectModel = new TypeWaveModel(mainManager, (TypeWaveSettings) EffectSettings);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public static string Name => "Type Waves";

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}