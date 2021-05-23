using System;
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
            ColorStopViewModels.CollectionChanged += ColorStopViewModelsOnCollectionChanged;
        }

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
        public bool HasMoreThanOneStop => ColorStopViewModels.Count > 1;
        private bool popupOpen = false;
        public bool ClearGradientPopupOpen => popupOpen;

        public ColorGradient ColorGradient { get; }

        public double PreviewWidth
        {
            get => _previewWidth;
            set => SetAndNotify(ref _previewWidth, value);
        }

        public ColorGradient Stops => ColorGradient;

        #region Overrides of DialogViewModelBase

        /// <inheritdoc />
        public override void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
            ColorGradient.CollectionChanged -= ColorGradientOnCollectionChanged;
            ColorStopViewModels.CollectionChanged -= ColorStopViewModelsOnCollectionChanged;
            base.OnDialogClosed(sender, e);
        }

        #endregion

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
            NotifyOfPropertyChange(nameof(HasMoreThanOneStop));
        }

        public void RemoveColorStop(ColorStopViewModel? colorStopViewModel)
        {
            // Can be null when called by the view
            if (colorStopViewModel == null)
                return;

            ColorStopViewModels.Remove(colorStopViewModel);
            ColorGradient.Remove(colorStopViewModel.ColorStop);

            SelectColorStop(null);
            NotifyOfPropertyChange(nameof(HasMoreThanOneStop));
        }

        #region Gradient Tools
        public void SpreadColorStops()
        {
            List<ColorStopViewModel> stops = ColorStopViewModels.OrderBy(x => x.OffsetFloat).ToList();
            int index = 0;
            foreach (ColorStopViewModel stop in stops)
            {
                stop.OffsetFloat = index / ((float) stops.Count - 1);
                index++;
            }
        }

        public void RotateColorStops()
        {
            List<ColorStopViewModel> stops;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) 
                stops = ColorStopViewModels.OrderBy(x => x.OffsetFloat).ToList();
            else 
                stops = ColorStopViewModels.OrderByDescending(x => x.OffsetFloat).ToList();

            float lastStopPosition = stops.Last().OffsetFloat;
            foreach (ColorStopViewModel stop in stops)
            {
                float tempStop = stop.OffsetFloat;
                stop.OffsetFloat = lastStopPosition;
                lastStopPosition = tempStop;
            }
        }

        public void FlipColorStops()
        {
            foreach (ColorStopViewModel stop in ColorStopViewModels) stop.OffsetFloat = 1 - stop.OffsetFloat;
        }

        public void ToggleSeam()
        {
            if (ColorGradient.IsSeamless())
            {
                // Remove the last stop
                ColorStopViewModel? stopToRemove = ColorStopViewModels.OrderBy(x => x.OffsetFloat).Last();

                if (stopToRemove == SelectedColorStopViewModel) SelectColorStop(null);

                ColorStopViewModels.Remove(stopToRemove);
                ColorGradient.Remove(stopToRemove.ColorStop);

                // Uncompress the stops if there is still more than one
                List<ColorStopViewModel> stops = ColorStopViewModels.OrderBy(x => x.OffsetFloat).ToList();

                if (stops.Count >= 2)
                {
                    float multiplier = stops.Count/(stops.Count - 1f);
                    foreach (ColorStopViewModel stop in stops)
                        stop.OffsetFloat = Math.Min(stop.OffsetFloat * multiplier, 100f);
                }
                

            }
            else
            {
                // Compress existing stops to the left
                List<ColorStopViewModel> stops = ColorStopViewModels.OrderBy(x => x.OffsetFloat).ToList();

                float multiplier = (stops.Count - 1f)/stops.Count;
                foreach (ColorStopViewModel stop in stops)
                    stop.OffsetFloat *= multiplier;

                // Add a stop to the end that is the same color as the first stop
                ColorGradientStop newStop = new(ColorGradient.First().Color, 1f);
                ColorGradient.Add(newStop);

                int index = ColorGradient.IndexOf(newStop);
                ColorStopViewModel viewModel = new(this, newStop);
                ColorStopViewModels.Insert(index, viewModel);
            }
        }

        public void ShowClearGradientPopup()
        {
            popupOpen = true;
            NotifyOfPropertyChange(nameof(ClearGradientPopupOpen));
        }
        public void HideClearGradientPopup()
        {
            popupOpen = false;
            NotifyOfPropertyChange(nameof(ClearGradientPopupOpen));
        }
        public void ClearGradientAndHide()
        {
            ClearGradient();
            HideClearGradientPopup();
        }
        public void ClearGradient()
        {
            ColorGradient.Clear();
            ColorStopViewModels.Clear();
        }

        #endregion

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

        private void ColorStopViewModelsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(HasMoreThanOneStop));
        }
    }
}