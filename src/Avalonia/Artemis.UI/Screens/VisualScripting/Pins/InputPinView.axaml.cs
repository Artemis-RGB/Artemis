using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.VisualScripting.Pins
{
    public partial class InputPinView : ReactiveUserControl<PinViewModel>
    {
        private bool _dragging;
        private readonly Border _pinPoint;
        private Canvas? _container;

        public InputPinView()
        {
            InitializeComponent();
            _pinPoint = this.Get<Border>("PinPoint");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void PinPoint_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            ZoomBorder? zoomBorder = this.FindAncestorOfType<ZoomBorder>();
            PointerPoint point = e.GetCurrentPoint(zoomBorder);
            if (ViewModel == null || zoomBorder == null || !point.Properties.IsLeftButtonPressed)
                return;

            if (!_dragging)
            {
                e.Pointer.Capture(_pinPoint);
                // ViewModel.StartDrag();
            }

            PointerPoint absolutePosition = e.GetCurrentPoint(null);
            OutputPinView? target = (OutputPinView?) zoomBorder.GetLogicalDescendants().FirstOrDefault(d => d is OutputPinView v && v.TransformedBounds != null && v.TransformedBounds.Value.Contains(absolutePosition.Position));

            // ViewModel.UpdateDrag(point.Position, target?.ViewModel);
            e.Handled = true;
        }

        private void PinPoint_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_dragging)
                return;

            _dragging = false;
            e.Pointer.Capture(null);
            // ViewModel.FinishDrag();
            e.Handled = true;
        }

        /// <inheritdoc />
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _container = this.FindAncestorOfType<Canvas>();
        }

        /// <inheritdoc />
        public override void Render(DrawingContext context)
        {
            base.Render(context);
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_container == null || ViewModel == null)
                return;

            Matrix? transform = this.TransformToVisual(_container);
            if (transform != null)
                ViewModel.Position = new Point(Bounds.Width / 2, Bounds.Height / 2).Transform(transform.Value);
        }
    }
}
