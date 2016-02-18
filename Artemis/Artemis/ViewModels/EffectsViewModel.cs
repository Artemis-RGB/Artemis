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
        private readonly TypeWaveViewModel _typeWaveVm;
        private readonly TypeHoleViewModel _typeHoleVm;
        private readonly AudioVisualizerViewModel _audioVisualizerVm;
        private readonly DebugEffectViewModel _debugVm;

        public EffectsViewModel(MainModel mainModel)
        {
            _typeWaveVm = new TypeWaveViewModel(mainModel) {DisplayName = "Type Waves"};
            _typeHoleVm = new TypeHoleViewModel(mainModel) { DisplayName = "Type Holes (NYI)" };
            _audioVisualizerVm = new AudioVisualizerViewModel(mainModel) { DisplayName = "Audio Visualization" };
            _debugVm = new DebugEffectViewModel(mainModel) { DisplayName = "Debug Effect" };
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateItem(_typeWaveVm);
            ActivateItem(_typeHoleVm);
            ActivateItem(_audioVisualizerVm);
            ActivateItem(_debugVm);
        }
    }
}