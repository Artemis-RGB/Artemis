using Artemis.Events;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public class AudioVisualizerViewModel : Screen, IHandle<ChangeActiveEffect>
    {
        private AudioVisualizerSettings _audioVisualizerSettings;

        public AudioVisualizerViewModel(MainModel mainModel)
        {
            // Subscribe to main model
            MainModel = mainModel;
            MainModel.Events.Subscribe(this);

            // Settings are loaded from file by class
            AudioVisualizerSettings = new AudioVisualizerSettings();

            // Create effect model and add it to MainModel
            AudioVisualizerModel = new AudioVisualizerModel(mainModel, AudioVisualizerSettings);
            MainModel.EffectModels.Add(AudioVisualizerModel);
        }

        public MainModel MainModel { get; set; }
        public AudioVisualizerModel AudioVisualizerModel { get; set; }

        public static string Name => "Audio Visualizer";
        public bool EffectEnabled => MainModel.IsEnabled(AudioVisualizerModel);

        public AudioVisualizerSettings AudioVisualizerSettings
        {
            get { return _audioVisualizerSettings; }
            set
            {
                if (Equals(value, _audioVisualizerSettings)) return;
                _audioVisualizerSettings = value;
                NotifyOfPropertyChange(() => AudioVisualizerSettings);
            }
        }

        public void Handle(ChangeActiveEffect message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }

        public void ToggleEffect()
        {
            if (EffectEnabled && !MainModel.Suspended)
                MainModel.ToggleSuspension();
            else if (!EffectEnabled && !MainModel.Suspended)
                MainModel.EnableEffect(AudioVisualizerModel);
            else
            {
                MainModel.ToggleSuspension();
                MainModel.EnableEffect(AudioVisualizerModel);
            }
        }

        public void SaveSettings()
        {
            AudioVisualizerSettings?.Save();
        }

        public void ResetSettings()
        {
            // TODO: Confirmation dialog (Generic MVVM approach)
            AudioVisualizerSettings.ToDefault();
            NotifyOfPropertyChange(() => AudioVisualizerSettings);

            SaveSettings();
        }
    }
}