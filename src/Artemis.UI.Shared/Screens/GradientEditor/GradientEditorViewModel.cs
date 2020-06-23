using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core.Models.Profile.Colors;
using Artemis.UI.Shared.Services.Dialog;
using Artemis.UI.Shared.Utilities;
using Stylet;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    public class GradientEditorViewModel : DialogViewModelBase
    {
        private readonly List<ColorGradientStop> _originalStops;
        private ColorStopViewModel _selectedColorStopViewModel;
        private double _previewWidth;

        public GradientEditorViewModel(ColorGradient colorGradient)
        {
            ColorGradient = colorGradient;
            ColorStopViewModels = new BindableCollection<ColorStopViewModel>();

            _originalStops = ColorGradient.Stops.Select(s => new ColorGradientStop(s.Color, s.Position)).ToList();

            PropertyChanged += UpdateColorStopViewModels;
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

        public double PreviewWidth
        {
            get => _previewWidth;
            set => SetAndNotify(ref _previewWidth, value);
        }

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

        public void Confirm()
        {
            if (!Session.IsEnded)
                Session.Close(true);
        }

        public void Cancel()
        {
            // Restore the saved state
            ColorGradient.Stops.Clear();
            ColorGradient.Stops.AddRange(_originalStops);
            ColorGradient.OnColorValuesUpdated();

            if (!Session.IsEnded)
                Session.Close(false);
        }

        private void UpdateColorStopViewModels(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PreviewWidth)) return;
            foreach (var colorStop in ColorGradient.Stops.OrderBy(s => s.Position))
                ColorStopViewModels.Add(new ColorStopViewModel(this, colorStop));
        }
    }
}