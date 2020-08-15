using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Artemis.Core.Models.Surface;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Visualization
{
    public class ProfileDeviceViewModel : CanvasViewModel
    {
        private ObservableCollection<ProfileLedViewModel> _leds;
        private ArtemisDevice _device;
        private bool _addedLeds;

        public ProfileDeviceViewModel(ArtemisDevice device)
        {
            Device = device;
            Leds = new ObservableCollection<ProfileLedViewModel>();

            Task.Run(AddLedsAsync);
        }

        public ObservableCollection<ProfileLedViewModel> Leds
        {
            get => _leds;
            set => SetAndNotify(ref _leds, value);
        }

        public ArtemisDevice Device
        {
            get => _device;
            set => SetAndNotify(ref _device, value);
        }

        public bool AddedLeds
        {
            get => _addedLeds;
            private set => SetAndNotify(ref _addedLeds, value);
        }

        public new double X
        {
            get => Device.X;
            set => Device.X = value;
        }

        public new double Y
        {
            get => Device.Y;
            set => Device.Y = value;
        }

        public int ZIndex
        {
            get => Device.ZIndex;
            set => Device.ZIndex = value;
        }


        public Rect DeviceRectangle => Device.RgbDevice == null
            ? new Rect()
            : new Rect(X, Y, Device.RgbDevice.Size.Width, Device.RgbDevice.Size.Height);

        /// <summary>
        ///     Update the color of all LEDs if finished adding
        /// </summary>
        public void Update()
        {
            if (!AddedLeds)
                return;

            foreach (var ledViewModel in Leds)
                ledViewModel.Update();
        }

        /// <summary>
        ///     Adds LEDs in batches of 5 to avoid UI freezes
        /// </summary>
        /// <returns></returns>
        private async Task AddLedsAsync()
        {
            var index = 0;
            foreach (var led in Device.Leds.ToList())
            {
                Execute.OnUIThreadSync(() => Leds.Add(new ProfileLedViewModel(led)));
                if (index % 5 == 0)
                    await Task.Delay(1);

                index++;
            }

            AddedLeds = true;
        }
    }
}