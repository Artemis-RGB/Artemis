using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Utilities;
using Stylet;

namespace Artemis.UI.Shared.Screens.GradientEditor
{
    public class ColorStopViewModel : PropertyChangedBase
    {
        private readonly GradientEditorViewModel _gradientEditorViewModel;
        private bool _isSelected;
        private bool _willRemoveColorStop;

        public ColorStopViewModel(GradientEditorViewModel gradientEditorViewModel, ColorGradientStop colorStop)
        {
            _gradientEditorViewModel = gradientEditorViewModel;
            ColorStop = colorStop;
            ColorStop.PropertyChanged += ColorStopOnPropertyChanged;
        }

        private void ColorStopOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _gradientEditorViewModel.ColorGradient.OnColorValuesUpdated();
        }

        public ColorGradientStop ColorStop { get; }

        public double Offset
        {
            get => ColorStop.Position * _gradientEditorViewModel.PreviewWidth;
            set
            {
                ColorStop.Position = (float) Math.Round(value / _gradientEditorViewModel.PreviewWidth, 3, MidpointRounding.AwayFromZero);
                NotifyOfPropertyChange(nameof(Offset));
                NotifyOfPropertyChange(nameof(OffsetPercent));
            }
        }

        public float OffsetPercent
        {
            get => (float) Math.Round(ColorStop.Position * 100.0, MidpointRounding.AwayFromZero);
            set
            {
                ColorStop.Position = Math.Min(100, Math.Max(0, value)) / 100f;
                NotifyOfPropertyChange(nameof(Offset));
                NotifyOfPropertyChange(nameof(OffsetPercent));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndNotify(ref _isSelected, value);
        }

        public bool WillRemoveColorStop
        {
            get => _willRemoveColorStop;
            set => SetAndNotify(ref _willRemoveColorStop, value);
        }

        #region Movement
        
        public void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            ((IInputElement) sender).CaptureMouse();
            _gradientEditorViewModel.SelectColorStop(this);
        }

        public void StopMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            ((IInputElement) sender).ReleaseMouseCapture();
            if (WillRemoveColorStop)
                _gradientEditorViewModel.RemoveColorStop(this);
        }

        public void StopMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (!((IInputElement) sender).IsMouseCaptured)
                return;

            var parent = VisualTreeUtilities.FindParent<Canvas>((DependencyObject) sender, null);
            var position = e.GetPosition(parent);
            if (position.Y > 50)
            {
                WillRemoveColorStop = true;
                return;
            }

            WillRemoveColorStop = false;

            var minValue = 0.0;
            var maxValue = _gradientEditorViewModel.PreviewWidth;
            var stops = _gradientEditorViewModel.ColorGradient.Stops.OrderBy(s => s.Position).ToList();
            var previous = stops.IndexOf(ColorStop) >= 1 ? stops[stops.IndexOf(ColorStop) - 1] : null;
            var next = stops.IndexOf(ColorStop) + 1 < stops.Count ? stops[stops.IndexOf(ColorStop) + 1] : null;
            if (previous != null)
                minValue = previous.Position * _gradientEditorViewModel.PreviewWidth;
            if (next != null)
                maxValue = next.Position * _gradientEditorViewModel.PreviewWidth;

            Offset = Math.Max(minValue, Math.Min(maxValue, position.X));
            _gradientEditorViewModel.ColorGradient.OnColorValuesUpdated();
        }

        #endregion
    }
}