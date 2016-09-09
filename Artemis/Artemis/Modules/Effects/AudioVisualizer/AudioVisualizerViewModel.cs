using Artemis.Managers;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public sealed class AudioVisualizerViewModel : EffectViewModel
    {
        public AudioVisualizerViewModel(MainManager main, ProfilePreviewModel model) : base(main, model)
        {
            DisplayName = "Audio Visualization / Key waves";
        }
    }
}