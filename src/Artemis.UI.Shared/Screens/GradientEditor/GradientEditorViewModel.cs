using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Utilities;
using Stylet;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    public class GradientEditorViewModel : Screen
    {
        private ColorStopViewModel _selectedColorStopViewModel;

        public GradientEditorViewModel(ColorGradient colorGradient)
        {
            ColorGradient = colorGradient;
            ColorStopViewModels = new BindableCollection<ColorStopViewModel>();

            foreach (var colorStop in ColorGradient.Stops.OrderBy(s => s.Position))
                ColorStopViewModels.Add(new ColorStopViewModel(this, colorStop));
        }

        public BindableCollection<ColorStopViewModel> ColorStopViewModels { get; set; }

        public ColorStopViewModel SelectedColorStopViewModel
        {
            get => _selectedColorStopViewModel;
            set
            {
                SetAndNotify(ref _selectedColorStopViewModel, value);
                NotifyOfPropertyChange(nameof(HasSelectedColorStopViewModel));
            }
        }

        public bool HasSelectedColorStopViewModel => SelectedColorStopViewModel != null;

        public ColorGradient ColorGradient { get; }
        public double PreviewWidth => 437.5;

        public void AddColorStop(object sender, MouseEventArgs e)
        {
            var child = VisualTreeUtilities.FindChild<Canvas>((DependencyObject) sender, null);
            var position = (float) (e.GetPosition(child).X / PreviewWidth);
            var stop = new ColorGradientStop(ColorGradient.GetColor(position), position);
            ColorGradient.Stops.Add(stop);
            ColorGradient.OnColorValuesUpdated();

            var index = ColorGradient.Stops.OrderBy(s => s.Position).ToList().IndexOf(stop);
            var viewModel = new ColorStopViewModel(this, stop);
            ColorStopViewModels.Insert(index, viewModel);

            SelectColorStop(viewModel);
        }

        public void RemoveColorStop(ColorStopViewModel colorStopViewModel)
        {
            ColorStopViewModels.Remove(colorStopViewModel);
            ColorGradient.Stops.Remove(colorStopViewModel.ColorStop);
            ColorGradient.OnColorValuesUpdated();

            SelectColorStop(null);
        }

        public Point GetPositionInPreview(object sender, MouseEventArgs e)
        {
            var parent = VisualTreeUtilities.FindParent<Canvas>((DependencyObject) sender, null);
            return e.GetPosition(parent);
        }

        public void SelectColorStop(ColorStopViewModel colorStopViewModel)
        {
            SelectedColorStopViewModel = colorStopViewModel;
            foreach (var stopViewModel in ColorStopViewModels)
                stopViewModel.IsSelected = stopViewModel == SelectedColorStopViewModel;
        }
    }
}