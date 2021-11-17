using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Artemis.UI.Shared;
using Artemis.VisualScripting.Editor.Controls.Wrapper;

namespace Artemis.VisualScripting.Editor.Controls
{
    [TemplatePart(Name = PART_PATH, Type = typeof(Path))]
    public class VisualScriptCablePresenter : Control
    {
        #region Constants

        private const string PART_PATH = "PART_Path";

        #endregion

        #region Properties & Fields

        private Path _path;
        private Border _valueBorder;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CableProperty = DependencyProperty.Register(
            "Cable", typeof(VisualScriptCable), typeof(VisualScriptCablePresenter), new PropertyMetadata(default(VisualScriptCable), CableChanged));

        public VisualScriptCable Cable
        {
            get => (VisualScriptCable) GetValue(CableProperty);
            set => SetValue(CableProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(
            "Thickness", typeof(double), typeof(VisualScriptCablePresenter), new PropertyMetadata(default(double)));

        public double Thickness
        {
            get => (double) GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        public static readonly DependencyProperty ValuePositionProperty = DependencyProperty.Register(
            "ValuePosition", typeof(Point), typeof(VisualScriptCablePresenter), new PropertyMetadata(default(Point)));

        public Point ValuePosition
        {
            get => (Point) GetValue(ValuePositionProperty);
            set => SetValue(ValuePositionProperty, value);
        }

        public static readonly DependencyProperty AlwaysShowValuesProperty = DependencyProperty.Register(
            "AlwaysShowValues", typeof(bool), typeof(VisualScriptCablePresenter), new PropertyMetadata(default(bool), AlwaysShowValuesChanged));

        public bool AlwaysShowValues
        {
            get => (bool) GetValue(AlwaysShowValuesProperty);
            set => SetValue(AlwaysShowValuesProperty, value);
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            _path = GetTemplateChild(PART_PATH) as Path ?? throw new NullReferenceException($"The Path '{PART_PATH}' is missing.");
            _valueBorder = GetTemplateChild("PART_ValueDisplay") as Border ?? throw new NullReferenceException("The Border 'PART_ValueDisplay' is missing.");

            _path.MouseEnter += (_, _) => UpdateValueVisibility();
            _path.MouseLeave += (_, _) => UpdateValueVisibility();
            _valueBorder.MouseEnter += (_, _) => UpdateValueVisibility();
            _valueBorder.MouseLeave += (_, _) => UpdateValueVisibility();
            Unloaded += OnUnloaded;

            UpdateValueVisibility();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (Cable?.From != null)
                Cable.From.PropertyChanged -= OnPinPropertyChanged;
            if (Cable?.To != null)
                Cable.To.PropertyChanged -= OnPinPropertyChanged;
        }

        private void OnPathMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left && args.LeftButton == MouseButtonState.Pressed && args.ClickCount == 2)
            {
                //TODO DarthAffe 17.06.2021: Should we add rerouting?
                //AddRerouteNode();
            }
            else if (args.ChangedButton == MouseButton.Middle)
            {
                Cable.Disconnect();
            }
        }

        private static void CableChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is not VisualScriptCablePresenter presenter) return;

            presenter.CableChanged(args);
        }

        private void CableChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue is VisualScriptCable oldCable)
            {
                oldCable.From.PropertyChanged -= OnPinPropertyChanged;
                oldCable.To.PropertyChanged -= OnPinPropertyChanged;
            }

            if (args.NewValue is VisualScriptCable newCable)
            {
                newCable.From.PropertyChanged += OnPinPropertyChanged;
                newCable.To.PropertyChanged += OnPinPropertyChanged;
            }

            UpdateBorderBrush();
            UpdateValuePosition();
        }

        private static void AlwaysShowValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is not VisualScriptCablePresenter presenter) return;

            presenter.UpdateValueVisibility();
        }

        private void UpdateValueVisibility()
        {
            if (_valueBorder == null)
                return;

            if (AlwaysShowValues && Cable.From.Connections.LastOrDefault() == Cable)
                _valueBorder.Visibility = Visibility.Visible;
            else if (_valueBorder.IsMouseOver || _path.IsMouseOver)
                _valueBorder.Visibility = Visibility.Visible;
            else
                _valueBorder.Visibility = Visibility.Collapsed;
        }

        private void OnPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VisualScriptPin.AbsolutePosition))
                UpdateValuePosition();
        }

        private void UpdateBorderBrush()
        {
            (Color color, Color _) = TypeUtilities.GetTypeColors(Cable.From.Pin.Type);
            BorderBrush = new SolidColorBrush(color);
        }

        private void UpdateValuePosition()
        {
            if (Cable.From == null || Cable.To == null)
                return;

            ValuePosition = new Point(
                Cable.From.AbsolutePosition.X + (Cable.To.AbsolutePosition.X - Cable.From.AbsolutePosition.X) / 2,
                Cable.From.AbsolutePosition.Y + (Cable.To.AbsolutePosition.Y - Cable.From.AbsolutePosition.Y) / 2
            );
        }

        #endregion
    }
}