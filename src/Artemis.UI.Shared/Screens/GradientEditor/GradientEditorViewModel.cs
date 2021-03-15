using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    internal class GradientEditorViewModel : DialogViewModelBase
    {
        private readonly List<ColorGradientStop> _originalStops;
        private double _previewWidth;
        private ColorStopViewModel? _selectedColorStopViewModel;

        public GradientEditorViewModel(ColorGradient colorGradient)
        {
            ColorGradient = colorGradient;
            ColorStopViewModels = new BindableCollection<ColorStopViewModel>();

            _originalStops = ColorGradient.Stops.Select(s => new ColorGradientStop(s.Color, s.Position)).ToList();

            PropertyChanged += UpdateColorStopViewModels;
        }

        public BindableCollection<ColorStopViewModel> ColorStopViewModels { get; set; }

        public ColorStopViewModel? SelectedColorStopViewModel
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
            Canvas? child = VisualTreeUtilities.FindChild<Canvas>((DependencyObject) sender, null);
            float position = (float) (e.GetPosition(child).X / PreviewWidth);
            ColorGradientStop stop = new(ColorGradient.GetColor(position), position);
            ColorGradient.Stops.Add(stop);
            ColorGradient.OnColorValuesUpdated();

            int index = ColorGradient.Stops.OrderBy(s => s.Position).ToList().IndexOf(stop);
            ColorStopViewModel viewModel = new(this, stop);
            ColorStopViewModels.Insert(index, viewModel);

            SelectColorStop(viewModel);
        }

        public void RemoveColorStop(ColorStopViewModel colorStopViewModel)
        {
            if (colorStopViewModel == null)
                return;

            ColorStopViewModels.Remove(colorStopViewModel);
            ColorGradient.Stops.Remove(colorStopViewModel.ColorStop);
            ColorGradient.OnColorValuesUpdated();

            SelectColorStop(null);
        }

        public Point GetPositionInPreview(object sender, MouseEventArgs e)
        {
            Canvas? parent = VisualTreeUtilities.FindParent<Canvas>((DependencyObject) sender, null);
            return e.GetPosition(parent);
        }

        public void SelectColorStop(ColorStopViewModel? colorStopViewModel)
        {
            SelectedColorStopViewModel = colorStopViewModel;
            foreach (ColorStopViewModel stopViewModel in ColorStopViewModels)
                stopViewModel.IsSelected = stopViewModel == SelectedColorStopViewModel;
        }

        public void Confirm()
        {
            if (Session != null && !Session.IsEnded)
                Session.Close(true);
        }

        public override void Cancel()
        {
            // Restore the saved state
            ColorGradient.Stops.Clear();
            ColorGradient.Stops.AddRange(_originalStops);
            ColorGradient.OnColorValuesUpdated();

            base.Cancel();
        }

        private void UpdateColorStopViewModels(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PreviewWidth)) return;
            foreach (ColorGradientStop colorStop in ColorGradient.Stops.OrderBy(s => s.Position))
                ColorStopViewModels.Add(new ColorStopViewModel(this, colorStop));
        }
    }
}