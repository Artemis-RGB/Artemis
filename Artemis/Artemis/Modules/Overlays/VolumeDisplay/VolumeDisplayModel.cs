using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Artemis.Models;
using Gma.System.MouseKeyHook;
using NAudio.CoreAudioApi;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplayModel : OverlayModel
    {
        private IKeyboardMouseEvents _mGlobalHook;

        public VolumeDisplayModel(MainModel mainModel, VolumeDisplaySettings settings) : base(mainModel)
        {
            Settings = settings;
            Name = "VolumeDisplay";
            Enabled = Settings.Enabled;

            VolumeDisplay = new VolumeDisplay(mainModel, settings);
        }

        public VolumeDisplay VolumeDisplay { get; set; }

        public VolumeDisplaySettings Settings { get; set; }

        public override void Dispose()
        {
            MainModel.KeyboardHook.Unsubscribe(HandleKeypress);
        }

        public override void Enable()
        {
            // Listener won't start unless the effect is active
            MainModel.KeyboardHook.Subscribe(HandleKeypress);
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
            return GenerateBitmap(MainModel.ActiveKeyboard.KeyboardBitmap(4));
        }

        public override Bitmap GenerateBitmap(Bitmap bitmap)
        {
            if (VolumeDisplay == null)
                return bitmap;
            if (VolumeDisplay.Ttl < 1)
                return bitmap;

            using (var g = Graphics.FromImage(bitmap))
            {
                VolumeDisplay.Draw(g);
            }

            return bitmap;
        }

        private void HandleKeypress(object sender, KeyEventArgs e)
        {
            Task.Factory.StartNew(() => KeyPressTask(e));
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