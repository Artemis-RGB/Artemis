using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
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

            _originalStops = ColorGradient.Select(s => new ColorGradientStop(s.Color, s.Position)).ToList();

            PropertyChanged += UpdateColorStopViewModels;
            ColorGradient.CollectionChanged += ColorGradientOnCollectionChanged;
        }

        #region Overrides of DialogViewModelBase

        /// <inheritdoc />
        public override void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
            ColorGradient.CollectionChanged -= ColorGradientOnCollectionChanged;
            base.OnDialogClosed(sender, e);
        }

        #endregion

        public BindableCollection<ColorStopViewModel> ColorStopViewModels { get; }

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

        public ColorGradient Stops
        {
            get => ColorGradient;
        }

        public void AddColorStop(object sender, MouseEventArgs e)
        {
            Canvas? child = VisualTreeUtilities.FindChild<Canvas>((DependencyObject) sender, null);
            float position = (float) (e.GetPosition(child).X / PreviewWidth);
            ColorGradientStop stop = new(ColorGradient.GetColor(position), position);
            ColorGradient.Add(stop);

            int index = ColorGradient.IndexOf(stop);
            ColorStopViewModel viewModel = new(this, stop);
            ColorStopViewModels.Insert(index, viewModel);

            SelectColorStop(viewModel);
        }

        public void RemoveColorStop(ColorStopViewModel colorStopViewModel)
        {
            if (colorStopViewModel == null)
                return;

            ColorStopViewModels.Remove(colorStopViewModel);
            ColorGradient.Remove(colorStopViewModel.ColorStop);

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
            ColorGradient.Clear();
            foreach (ColorGradientStop colorGradientStop in _originalStops)
                ColorGradient.Add(colorGradientStop);

            base.Cancel();
        }

        private void UpdateColorStopViewModels(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PreviewWidth)) return;
            foreach (ColorGradientStop colorStop in ColorGradient)
                ColorStopViewModels.Add(new ColorStopViewModel(this, colorStop));
        }
        
        private void ColorGradientOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(ColorGradient));
        }
    }
}