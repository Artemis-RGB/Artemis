using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.VisualScripting.Editor.Controls.Wrapper;

namespace Artemis.VisualScripting.Editor.Controls
{
    [TemplatePart(Name = PART_DOT, Type = typeof(FrameworkElement))]
    public class VisualScriptPinPresenter : Control
    {
        #region Constants

        private const string PART_DOT = "PART_Dot";

        #endregion

        #region Properties & Fields

        private VisualScriptNodePresenter _nodePresenter;
        private FrameworkElement _dot;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty PinProperty = DependencyProperty.Register(
            "Pin", typeof(VisualScriptPin), typeof(VisualScriptPinPresenter), new PropertyMetadata(default(VisualScriptPin), PinChanged));

        public VisualScriptPin Pin
        {
            get => (VisualScriptPin)GetValue(PinProperty);
            set => SetValue(PinProperty, value);
        }

        #endregion

        #region Constructors

        public VisualScriptPinPresenter()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion

        #region Methods

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            DependencyObject parent = this;
            while ((parent = VisualTreeHelper.GetParent(parent)) != null)
            {
                if (parent is VisualScriptNodePresenter nodePresenter)
                {
                    _nodePresenter = nodePresenter;
                    break;
                }
            }

            LayoutUpdated += OnLayoutUpdated;

            UpdateAbsoluteLocation();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            LayoutUpdated -= OnLayoutUpdated;
            if (Pin != null)
                Pin.Node.Node.PropertyChanged -= OnNodePropertyChanged;

            _nodePresenter = null;
        }

        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateAbsoluteLocation();
        }

        private void OnLayoutUpdated(object sender, EventArgs args)
        {
            _dot = GetTemplateChild(PART_DOT) as FrameworkElement ?? throw new NullReferenceException($"The Element '{PART_DOT}' is missing.");

            _dot.AllowDrop = true;

            _dot.MouseDown += OnDotMouseDown;
            _dot.MouseMove += OnDotMouseMove;
            _dot.Drop += OnDotDrop;
            _dot.DragEnter += OnDotDrag;
            _dot.DragOver += OnDotDrag;

            UpdateAbsoluteLocation();
        }

        private static void PinChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is not VisualScriptPinPresenter presenter) return;

            presenter.PinChanged(args);
        }

        private void PinChanged(DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue is VisualScriptPin oldPin)
                oldPin.Node.Node.PropertyChanged -= OnNodePropertyChanged;

            if (args.NewValue is VisualScriptPin newPin)
                newPin.Node.Node.PropertyChanged += OnNodePropertyChanged;

            UpdateBrushes();
            UpdateAbsoluteLocation();
        }

        private void UpdateBrushes()
        {
            (Color border, Color background) = TypeUtilities.GetTypeColors(Pin.Pin.Type);
            Background = new SolidColorBrush(background);
            BorderBrush = new SolidColorBrush(border);
        }

        private void UpdateAbsoluteLocation()
        {
            if ((Pin == null) || (_nodePresenter == null)) return;

            try
            {
                double circleRadius = ActualHeight / 2.0;
                double xOffset = Pin.Pin.Direction == PinDirection.Input ? circleRadius : ActualWidth - circleRadius;
                Point relativePosition = this.TransformToVisual(_nodePresenter).Transform(new Point(xOffset, circleRadius));
                Pin.AbsolutePosition = new Point(Pin.Node.X + relativePosition.X, Pin.Node.Y + relativePosition.Y);
            }
            catch
            {
                Pin.AbsolutePosition = new Point(0, 0);
            }
        }

        private void OnDotMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Middle)
                Pin.DisconnectAll();

            args.Handled = true;
        }

        private void OnDotMouseMove(object sender, MouseEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                Pin.SetConnecting(true);
                DragDrop.DoDragDrop(this, Pin, DragDropEffects.Link);
                Pin.SetConnecting(false);

                args.Handled = true;
            }
        }

        private void OnDotDrag(object sender, DragEventArgs args)
        {
            if (!args.Data.GetDataPresent(typeof(VisualScriptPin)))
                args.Effects = DragDropEffects.None;
            else
            {
                VisualScriptPin sourcePin = (VisualScriptPin)args.Data.GetData(typeof(VisualScriptPin));
                if (sourcePin == null)
                    args.Effects = DragDropEffects.None;
                else
                    args.Effects = ((sourcePin.Pin.Direction != Pin.Pin.Direction) && (sourcePin.Pin.Node != Pin.Pin.Node) && IsTypeCompatible(sourcePin.Pin.Type)) ? DragDropEffects.Link : DragDropEffects.None;
            }

            if (args.Effects == DragDropEffects.None)
                args.Handled = true;
        }

        private void OnDotDrop(object sender, DragEventArgs args)
        {
            if (!args.Data.GetDataPresent(typeof(VisualScriptPin))) return;

            VisualScriptPin sourcePin = (VisualScriptPin)args.Data.GetData(typeof(VisualScriptPin));
            if ((sourcePin == null) || !IsTypeCompatible(sourcePin.Pin.Type)) return;

            try { new VisualScriptCable(Pin, sourcePin); } catch { /**/ }

            args.Handled = true;
        }

        private bool IsTypeCompatible(Type type) => (Pin.Pin.Type == type)
                                                 || (Pin.Pin.Type == typeof(Enum) && type.IsEnum)
                                                 || ((Pin.Pin.Direction == PinDirection.Input) && (Pin.Pin.Type == typeof(object)))
                                                 || ((Pin.Pin.Direction == PinDirection.Output) && (type == typeof(object)));
        
        #endregion
    }
}
