using System;
using System.Windows.Input;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using FluentAvalonia.Styling;

namespace Artemis.UI.Avalonia.Shared.Controls
{
    /// <summary>
    ///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
    /// </summary>
    public class SelectionRectangle : Control
    {
        /// <summary>
        ///     Defines the <see cref="Background" /> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BackgroundProperty =
            AvaloniaProperty.Register<SelectionRectangle, IBrush>(nameof(Background),
                new SolidColorBrush(AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor ?? Colors.Transparent, 0.25));

        /// <summary>
        ///     Defines the <see cref="BorderBrush" /> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BorderBrushProperty =
            AvaloniaProperty.Register<SelectionRectangle, IBrush>(nameof(BorderBrush),
                new SolidColorBrush(AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().CustomAccentColor ?? Colors.Transparent));

        /// <summary>
        ///     Defines the <see cref="BorderBrush" /> property.
        /// </summary>
        public static readonly StyledProperty<double> BorderThicknessProperty =
            AvaloniaProperty.Register<SelectionRectangle, double>(nameof(BorderThickness), 1);

        /// <summary>
        ///     Defines the <see cref="get_InputElement" /> property.
        /// </summary>
        public static readonly StyledProperty<IControl?> InputElementProperty =
            AvaloniaProperty.Register<SelectionRectangle, IControl?>(nameof(InputElement), notifying: OnInputElementChanged);

        public static readonly StyledProperty<ICommand?> SelectionUpdatedProperty
            = AvaloniaProperty.Register<SelectionRectangle, ICommand?>(nameof(SelectionUpdated));

        public static readonly StyledProperty<ICommand?> SelectionFinishedProperty
            = AvaloniaProperty.Register<SelectionRectangle, ICommand?>(nameof(SelectionUpdated));

        private Rect? _displayRect;
        private IControl? _oldInputElement;
        private Point _startPosition;

        /// <inheritdoc />
        public SelectionRectangle()
        {
            AffectsRender<TextBlock>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty);
        }

        /// <summary>
        ///     Gets or sets a brush used to paint the control's background.
        /// </summary>
        public IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        ///     Gets or sets a brush used to paint the control's border
        /// </summary>
        public IBrush BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        /// <summary>
        ///     Gets or sets the width of the control's border
        /// </summary>
        public double BorderThickness
        {
            get => GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public IControl? InputElement
        {
            get => GetValue(InputElementProperty);
            set => SetValue(InputElementProperty, value);
        }

        public ICommand? SelectionUpdated
        {
            get => GetValue(SelectionUpdatedProperty);
            set => SetValue(SelectionUpdatedProperty, value);
        }

        public ICommand? SelectionFinished
        {
            get => GetValue(SelectionFinishedProperty);
            set => SetValue(SelectionFinishedProperty, value);
        }

        private static void OnInputElementChanged(IAvaloniaObject sender, bool before)
        {
            ((SelectionRectangle) sender).SubscribeToInputElement();
        }

        private void ParentOnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Pointer.Capture(this);

            _startPosition = e.GetPosition(Parent);
            _displayRect = null;
        }

        private void ParentOnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!ReferenceEquals(e.Pointer.Captured, this))
                return;

            Point currentPosition = e.GetPosition(Parent);
            _displayRect = new Rect(
                new Point(Math.Min(_startPosition.X, currentPosition.X), Math.Min(_startPosition.Y, currentPosition.Y)),
                new Point(Math.Max(_startPosition.X, currentPosition.X), Math.Max(_startPosition.Y, currentPosition.Y))
            );
            SelectionUpdated?.Execute(_displayRect.Value);

            InvalidateVisual();
        }

        private void ParentOnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            e.Pointer.Capture(null);

            if (_displayRect != null)
                SelectionFinished?.Execute(_displayRect.Value);

            _displayRect = null;
            InvalidateVisual();
        }

        private void SubscribeToInputElement()
        {
            if (_oldInputElement != null)
            {
                _oldInputElement.PointerPressed -= ParentOnPointerPressed;
                _oldInputElement.PointerMoved -= ParentOnPointerMoved;
                _oldInputElement.PointerReleased -= ParentOnPointerReleased;
            }

            _oldInputElement = InputElement;

            if (InputElement != null)
            {
                InputElement.PointerPressed += ParentOnPointerPressed;
                InputElement.PointerMoved += ParentOnPointerMoved;
                InputElement.PointerReleased += ParentOnPointerReleased;
            }
        }

        #region Overrides of Visual

        public override void Render(DrawingContext drawingContext)
        {
            if (_displayRect != null)
                drawingContext.DrawRectangle(Background, new Pen(BorderBrush, BorderThickness), _displayRect.Value);
        }

        /// <inheritdoc />
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            SubscribeToInputElement();
            base.OnAttachedToVisualTree(e);
        }

        /// <inheritdoc />
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            if (_oldInputElement != null)
            {
                _oldInputElement.PointerPressed -= ParentOnPointerPressed;
                _oldInputElement.PointerMoved -= ParentOnPointerMoved;
                _oldInputElement.PointerReleased -= ParentOnPointerReleased;
                _oldInputElement = null;
            }

            base.OnDetachedFromVisualTree(e);
        }

        #endregion
    }
}