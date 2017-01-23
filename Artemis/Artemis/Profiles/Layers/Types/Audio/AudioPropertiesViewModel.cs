using System.Linq;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.ViewModels;
using Caliburn.Micro;
using CSCore.CoreAudioAPI;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioPropertiesViewModel : LayerPropertiesViewModel
    {
        private ILayerAnimation _selectedLayerAnimation;

        public AudioPropertiesViewModel(LayerEditorViewModel editorVm) : base(editorVm)
        {
            LayerAnimations = new BindableCollection<ILayerAnimation>(editorVm.LayerAnimations);
            Devices = new BindableCollection<string>();
            SelectedLayerAnimation =
                LayerAnimations.FirstOrDefault(l => l.Name == editorVm.ProposedLayer.LayerAnimation?.Name) ??
                LayerAnimations.First(l => l.Name == "None");

            SetupAudioSelection();
            if (SelectedDevice == null)
                SelectedDevice = Devices.First();
        }

        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public BindableCollection<string> Devices { get; set; }

        public ILayerAnimation SelectedLayerAnimation
        {
            get { return _selectedLayerAnimation; }
            set
            {
                if (Equals(value, _selectedLayerAnimation)) return;
                _selectedLayerAnimation = value;
                NotifyOfPropertyChange(() => SelectedLayerAnimation);
            }
        }

        public MmDeviceType DeviceType
        {
            get { return ((AudioPropertiesModel) LayerModel.Properties).DeviceType; }
            set
            {
                if (value == ((AudioPropertiesModel) LayerModel.Properties).DeviceType) return;
                ((AudioPropertiesModel) LayerModel.Properties).DeviceType = value;
                SetupAudioSelection();
                SelectedDevice = Devices.First();
                NotifyOfPropertyChange(() => DeviceType);
            }
        }

        public string SelectedDevice
        {
            get { return ((AudioPropertiesModel) LayerModel.Properties).Device; }
            set
            {
                if (value == ((AudioPropertiesModel) LayerModel.Properties).Device) return;
                ((AudioPropertiesModel) LayerModel.Properties).Device = value;
                NotifyOfPropertyChange(() => SelectedDevice);
            }
        }

        private void SetupAudioSelection()
        {
            var properties = (AudioPropertiesModel) LayerModel.Properties;

            Devices.Clear();
            Devices.Add("Default");

            // Select the proper devices and make sure they are unique
            Devices.AddRange(properties.DeviceType == MmDeviceType.Input
                ? MMDeviceEnumerator.EnumerateDevices(DataFlow.Capture, DeviceState.Active)
                    .Select(d => d.FriendlyName).GroupBy(d => d).Select(g => g.First())
                : MMDeviceEnumerator.EnumerateDevices(DataFlow.Render, DeviceState.Active)
                    .Select(d => d.FriendlyName).GroupBy(d => d).Select(g => g.First()));
        }

        public override void ApplyProperties()
        {
            LayerModel.Properties.Brush = Brush;
            LayerModel.LayerAnimation = SelectedLayerAnimation;
        }
    }
}