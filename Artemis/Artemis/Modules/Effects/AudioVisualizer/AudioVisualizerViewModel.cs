using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public class AudioVisualizerViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public AudioVisualizerViewModel(MainManager mainManager)
        {
            // Subscribe to main model
            MainManager = mainManager;
            MainManager.Events.Subscribe(this);

            // Settings are loaded from file by class
            EffectSettings = new AudioVisualizerSettings();

            // Create effect model and add it to MainManager
            EffectModel = new AudioVisualizerModel(mainManager, (AudioVisualizerSettings) EffectSettings);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public static string Name => "Audio Visualizer";

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}