using Artemis.DAL;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Modules.General.GeneralProfile;
using CSCore.CoreAudioAPI;

namespace Artemis.Modules.Overlays.OverlayProfile
{
    public class OverlayProfileModel : ModuleModel
    {
        private AudioEndpointVolume _endPointVolume;

        public OverlayProfileModel(DeviceManager deviceManager, LuaManager luaManager,
            AudioCaptureManager audioCaptureManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<OverlayProfileSettings>();
            DataModel = new OverlayProfileDataModel();

            var defaultPlayback = MMDeviceEnumerator.TryGetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (defaultPlayback != null)
                _endPointVolume = AudioEndpointVolume.FromDevice(defaultPlayback);

            audioCaptureManager.AudioDeviceChanged += OnAudioDeviceChanged;

            Enable();
        }

        public override string Name => "OverlayProfile";
        public override bool IsOverlay => true;
        public override bool IsBoundToProcess => false;

        private void OnAudioDeviceChanged(object sender, AudioDeviceChangedEventArgs e)
        {
            if (e.DefaultPlayback != null)
                _endPointVolume = AudioEndpointVolume.FromDevice(e.DefaultPlayback);
        }

        public override void Update()
        {
            if (!Settings.IsEnabled)
                return;

            var dataModel = (OverlayProfileDataModel) DataModel;

            dataModel.Keyboard.NumLock = ((ushort) GeneralProfileModel.GetKeyState(0x90) & 0xffff) != 0;
            dataModel.Keyboard.CapsLock = ((ushort) GeneralProfileModel.GetKeyState(0x14) & 0xffff) != 0;
            dataModel.Keyboard.ScrollLock = ((ushort) GeneralProfileModel.GetKeyState(0x91) & 0xffff) != 0;

            if (_endPointVolume != null)
                dataModel.Audio.Volume = _endPointVolume.GetMasterVolumeLevelScalar();
        }

        public override void Render(RenderFrame frame, bool keyboardOnly)
        {
            if (Settings.IsEnabled)
                base.Render(frame, keyboardOnly);
        }

        public override void Dispose()
        {
            PreviewLayers = null;
        }
    }
}