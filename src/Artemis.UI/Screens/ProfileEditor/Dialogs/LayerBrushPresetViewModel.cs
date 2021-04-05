using System.Threading.Tasks;
using Artemis.Core.LayerBrushes;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    public class LayerBrushPresetViewModel : DialogViewModelBase
    {
        private ILayerBrushPreset _selectedPreset;

        public LayerBrushPresetViewModel(BaseLayerBrush layerBrush)
        {
            Presets = new BindableCollection<ILayerBrushPreset>();
            Presets.AddRange(layerBrush.Presets);
        }

        public BindableCollection<ILayerBrushPreset> Presets { get; }

        public ILayerBrushPreset SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                SetAndNotify(ref _selectedPreset, value);
                SelectPreset(value);
            }
        }

        public void SelectPreset(ILayerBrushPreset preset)
        {
            preset.Apply();
            Execute.OnUIThreadAsync(async () =>
            {
                await Task.Delay(250);
                Session?.Close(true);
            });
        }
    }
}