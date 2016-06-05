using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public sealed class AudioVisualizerViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public AudioVisualizerViewModel(MainManager main, IEventAggregator events)
            : base(main, new AudioVisualizerModel(main, new AudioVisualizerSettings()))
        {
            DisplayName = "Audio Visualization";

            events.Subscribe(this);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}