using Artemis.Models;
using Artemis.Modules.Effects.AudioVisualizer;
using Artemis.Modules.Effects.Debug;
using Artemis.Modules.Effects.TypeHole;
using Artemis.Modules.Effects.TypeWave;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class EffectsViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public EffectsViewModel(MainModel mainModel)
        {
            ActivateItem(new TypeWaveViewModel(mainModel) {DisplayName = "Type Waves"});
            ActivateItem(new TypeHoleViewModel(mainModel) {DisplayName = "Type Holes (NYI)"});
            ActivateItem(new AudioVisualizerViewModel(mainModel) {DisplayName = "Audio Visualization"});
            ActivateItem(new DebugEffectViewModel(mainModel) {DisplayName = "Debug Effect"});
        }
    }
}