using Artemis.Managers;
using Artemis.Modules.Effects.AmbientLightning;
using Artemis.Modules.Effects.AudioVisualizer;
using Artemis.Modules.Effects.Debug;
using Artemis.Modules.Effects.TypeHole;
using Artemis.Modules.Effects.TypeWave;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class EffectsViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly AudioVisualizerViewModel _audioVisualizerVm;
        private readonly DebugEffectViewModel _debugVm;
        private readonly TypeHoleViewModel _typeHoleVm;
        private readonly TypeWaveViewModel _typeWaveVm;
        //private readonly AmbientLightningEffectViewModel _ambientLightningVm;

        public EffectsViewModel(MainManager mainManager)
        {
            _typeWaveVm = new TypeWaveViewModel(mainManager) {DisplayName = "Type Waves"};
            //_typeHoleVm = new TypeHoleViewModel(MainManager) {DisplayName = "Type Holes (NYI)"};
            _audioVisualizerVm = new AudioVisualizerViewModel(mainManager) {DisplayName = "Audio Visualization"};
            //_ambientLightningVm = new AmbientLightningEffectViewModel(mainManager) {DisplayName = "Ambient Lightning"};
            _debugVm = new DebugEffectViewModel(mainManager) {DisplayName = "Debug Effect"};
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateItem(_typeWaveVm);
            //ActivateItem(_typeHoleVm);
            ActivateItem(_audioVisualizerVm);
            //ActivateItem(_ambientLightningVm);
            ActivateItem(_debugVm);
        }
    }
}