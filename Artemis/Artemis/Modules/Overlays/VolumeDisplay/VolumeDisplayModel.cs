using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Artemis.Managers;
using Artemis.Models;
using NAudio.CoreAudioApi;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplayModel : OverlayModel
    {
        public VolumeDisplayModel(MainManager mainManager, VolumeDisplaySettings settings) : base(mainManager)
        {
            Settings = settings;
            Name = "VolumeDisplay";
            Enabled = Settings.Enabled;

            VolumeDisplay = new VolumeBar(mainManager, settings);
        }

        public VolumeBar VolumeDisplay { get; set; }

        public VolumeDisplaySettings Settings { get; set; }

        public override void Dispose()
        {
            MainManager.KeyboardHook.KeyDownCallback -= KeyPressTask;
        }

        public override void Enable()
        {
            // Listener won't start unless the effect is active
            MainManager.KeyboardHook.KeyDownCallback += KeyPressTask;
        }

        public override void Update()
        {
            // TODO: Get from settings
            var fps = 25;

            if (VolumeDisplay == null)
                return;
            if (VolumeDisplay.Ttl < 1)
                return;

            var decreaseAmount = 500/fps;
            VolumeDisplay.Ttl = VolumeDisplay.Ttl - decreaseAmount;
            if (VolumeDisplay.Ttl < 128)
                VolumeDisplay.Transparancy = (byte) (VolumeDisplay.Transparancy - 20);

            try
            {
                var enumerator = new MMDeviceEnumerator();
                var volumeFloat =
                    enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)
                        .AudioEndpointVolume.MasterVolumeLevelScalar;
                VolumeDisplay.Volume = (int) (volumeFloat*100);
            }
            catch (COMException)
            {
            }
        }

        public override Bitmap GenerateBitmap()
        {
            return GenerateBitmap(MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(4));
        }

        public override Bitmap GenerateBitmap(Bitmap bitmap)
        {
            if (VolumeDisplay == null)
                return bitmap;
            if (VolumeDisplay.Ttl < 1)
                return bitmap;

            using (var g = Graphics.FromImage(bitmap))
                VolumeDisplay.Draw(g);

            return bitmap;
        }

        private void KeyPressTask(KeyEventArgs e)
        {
            if (e.KeyCode != Keys.VolumeUp && e.KeyCode != Keys.VolumeDown)
                return;

            VolumeDisplay.Ttl = 1000;
            VolumeDisplay.Transparancy = 255;
        }
    }
}