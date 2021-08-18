using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Artemis.Core;
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
            get => (Point)GetValue(ValuePositionProperty);
            set => SetValue(ValuePositionProperty, value);
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            _path = GetTemplateChild(PART_PATH) as Path ?? throw new NullReferenceException($"The Path '{PART_PATH}' is missing.");
            _path.MouseDown += OnPathMouseDown;
            
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (Cable?.From != null)
                Cable.From.PropertyChanged += OnPinPropertyChanged;
            if (Cable?.To != null)
                Cable.To.PropertyChanged += OnPinPropertyChanged;
        }

        private void OnPathMouseDown(object sender, MouseButtonEventArgs args)
        {
            if ((args.ChangedButton == MouseButton.Left) && (args.LeftButton == MouseButtonState.Pressed) && (args.ClickCount == 2))
            {
                //TODO DarthAffe 17.06.2021: Should we add rerouting?
                //AddRerouteNode();
            }
            else if (args.ChangedButton == MouseButton.Middle)
                Cable.Disconnect();
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
        
        private void OnPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VisualScriptPin.AbsolutePosition))
                UpdateValuePosition();
        }

        private void UpdateBorderBrush()
        {
            // if (Cable.From.Pin.Type.IsAssignableTo(typeof(IList)))
                // BorderBrush = new SolidColorBrush(Colors.MediumPurple);
        }

        private void UpdateValuePosition()
        {
            if (Cable.From == null || Cable.To == null)
                return;
            
            ValuePosition = new Point(
                Cable.From.AbsolutePosition.X + ((Cable.To.AbsolutePosition.X - Cable.From.AbsolutePosition.X) / 2),
                Cable.From.AbsolutePosition.Y + ((Cable.To.AbsolutePosition.Y - Cable.From.AbsolutePosition.Y) / 2)
            );
        }

        #endregion
    }
}