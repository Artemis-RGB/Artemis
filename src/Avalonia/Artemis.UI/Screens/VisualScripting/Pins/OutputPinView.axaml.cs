using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins
{
    public partial class OutputPinView : ReactiveUserControl<PinViewModel>
    {
        private Canvas? _container;

        public OutputPinView()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        public override void Render(DrawingContext context)
        {
            base.Render(context);
            UpdatePosition();
        }

        /// <inheritdoc />
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _container = this.FindAncestorOfType<Canvas>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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