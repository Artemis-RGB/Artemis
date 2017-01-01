using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities.Keyboard;
using NAudio.CoreAudioApi;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplayModel : OverlayModel
    {
        public VolumeDisplayModel(DeviceManager deviceManager, LuaManager luaManager)
            : base(deviceManager, luaManager, SettingsProvider.Load<VolumeDisplaySettings>())
        {
            Name = "VolumeDisplay";
            Settings = (VolumeDisplaySettings) base.Settings;
            VolumeDisplay = new VolumeBar(DeviceManager, Settings);
        }

        public new VolumeDisplaySettings Settings { get; set; }
        public VolumeBar VolumeDisplay { get; set; }

        public override void Dispose()
        {
            KeyboardHook.KeyDownCallback -= KeyPressTask;
        }

        public override void Enable()
        {
            // Listener won't start unless the effect is active
            KeyboardHook.KeyDownCallback += KeyPressTask;
        }

        public override void Update()
        {
            // TODO: Get from settings
            var fps = 25;

            if (VolumeDisplay == null)
                return;
            if (VolumeDisplay.Ttl < 1)
                return;

            var decreaseAmount = 500 / fps;
            VolumeDisplay.Ttl = VolumeDisplay.Ttl - decreaseAmount;
            if (VolumeDisplay.Ttl < 128)
                VolumeDisplay.Transparancy = (byte) (VolumeDisplay.Transparancy - 20);

            try
            {
                var enumerator = new MMDeviceEnumerator();
                var volumeFloat =
                    enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)
                        .AudioEndpointVolume.MasterVolumeLevelScalar;
                VolumeDisplay.Volume = (int) (volumeFloat * 100);
            }
            catch (COMException)
            {
            }
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return null;
        }

        private void KeyPressTask(KeyEventArgs e)
        {
            if ((e.KeyCode != Keys.VolumeUp) && (e.KeyCode != Keys.VolumeDown))
                return;

            VolumeDisplay.Ttl = 1000;
            VolumeDisplay.Transparancy = 255;
        }

        public override void RenderOverlay(RenderFrame frame, bool keyboardOnly)
        {
            if ((DeviceManager.ActiveKeyboard == null) || (VolumeDisplay == null) || (VolumeDisplay.Ttl < 1))
                return;

            using (var g = Graphics.FromImage(frame.KeyboardBitmap))
            {
                VolumeDisplay.Draw(g);
            }
        }
    }
}