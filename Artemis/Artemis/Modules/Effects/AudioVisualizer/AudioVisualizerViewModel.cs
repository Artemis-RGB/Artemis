using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public sealed class AudioVisualizerViewModel : EffectViewModel
    {
        public AudioVisualizerViewModel(MainManager main, AudioVisualizerModel model) : base(main, model)
        {
            DisplayName = "Audio Visualization";
        }
    }
}