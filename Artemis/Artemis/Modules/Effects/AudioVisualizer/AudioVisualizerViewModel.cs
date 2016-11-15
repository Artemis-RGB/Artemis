using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public sealed class AudioVisualizerViewModel : EffectViewModel
    {
        public AudioVisualizerViewModel(MainManager main, [Named("ProfilePreviewModel")] EffectModel model) : base(main, model)
        {
            DisplayName = "Audio Visualization / Key waves";
        }
    }
}