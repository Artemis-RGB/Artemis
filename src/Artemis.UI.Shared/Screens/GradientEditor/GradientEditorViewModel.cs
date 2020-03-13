using System.ComponentModel;
using System.Linq;
using Artemis.Core.Models.Profile;
using Stylet;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    public class GradientEditorViewModel : Screen
    {
        public GradientEditorViewModel(ColorGradient colorGradient)
        {
            ColorGradient = colorGradient;
            ColorStopViewModels = new BindableCollection<ColorStopViewModel>();

            ColorGradient.PropertyChanged += ColorGradientOnPropertyChanged;
            UpdateColorStopViewModels();
        }
        
        public BindableCollection<ColorStopViewModel> ColorStopViewModels { get; set; }

        public ColorGradient ColorGradient { get; }
        public double PreviewWidth => 440;

        private void ColorGradientOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColorGradient.Stops))
                UpdateColorStopViewModels();
        }

        private void UpdateColorStopViewModels()
        {
            while (ColorGradient.Stops.Count > ColorStopViewModels.Count)
                ColorStopViewModels.Add(new ColorStopViewModel(this));
            while (ColorGradient.Stops.Count < ColorStopViewModels.Count)
                ColorStopViewModels.RemoveAt(0);

            var index = 0;
            foreach (var colorStop in ColorGradient.Stops.OrderBy(s => s.Position))
            {
                var viewModel = ColorStopViewModels[index];
                viewModel.Update(colorStop);
                index++;
            }
        }
    }
}