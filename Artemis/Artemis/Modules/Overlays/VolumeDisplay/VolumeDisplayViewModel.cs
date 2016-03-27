using Artemis.Managers;
using Caliburn.Micro;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplayViewModel : Screen
    {
        private VolumeDisplaySettings _volumeDisplaySettings;

        public VolumeDisplayViewModel(MainManager mainManager)
        {
            MainManager = mainManager;

            // Settings are loaded from file by class
            VolumeDisplaySettings = new VolumeDisplaySettings();

            // Create effect model and add it to MainManager
            VolumeDisplayModel = new VolumeDisplayModel(mainManager, VolumeDisplaySettings);
            MainManager.EffectManager.EffectModels.Add(VolumeDisplayModel);
        }

        public static string Name => "Volume Display";

        public MainManager MainManager { get; set; }
        public VolumeDisplayModel VolumeDisplayModel { get; set; }

        public VolumeDisplaySettings VolumeDisplaySettings
        {
            get { return _volumeDisplaySettings; }
            set
            {
                if (Equals(value, _volumeDisplaySettings)) return;
                _volumeDisplaySettings = value;
                NotifyOfPropertyChange(() => VolumeDisplaySettings);
            }
        }

        public void ToggleEffect()
        {
            VolumeDisplayModel.Enabled = _volumeDisplaySettings.Enabled;
        }

        public void SaveSettings()
        {
            if (VolumeDisplayModel == null)
                return;

            VolumeDisplaySettings.Save();
        }

        public void ResetSettings()
        {
            // TODO: Confirmation dialog (Generic MVVM approach)
            VolumeDisplaySettings.ToDefault();
            NotifyOfPropertyChange(() => VolumeDisplaySettings);

            SaveSettings();
        }
    }
}