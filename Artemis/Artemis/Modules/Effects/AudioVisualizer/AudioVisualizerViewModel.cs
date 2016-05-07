using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public sealed class AudioVisualizerViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public AudioVisualizerViewModel(MainManager mainManager, KeyboardManager keyboardManager,
            EffectManager effectManager, IEventAggregator events)
            : base(
                mainManager, effectManager,
                new AudioVisualizerModel(mainManager, keyboardManager, new AudioVisualizerSettings()))
        {
            DisplayName = "Audio Visualization";

            events.Subscribe(this);
            EffectManager.EffectModels.Add(EffectModel);
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}