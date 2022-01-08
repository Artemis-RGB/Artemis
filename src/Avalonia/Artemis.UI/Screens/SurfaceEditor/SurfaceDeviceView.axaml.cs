using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.SurfaceEditor
{
    public class SurfaceDeviceView : ReactiveUserControl<SurfaceDeviceViewModel>
    {
        public SurfaceDeviceView()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        protected override void OnPointerEnter(PointerEventArgs e)
        {
            if (ViewModel?.SelectionStatus == SelectionStatus.None)
            {
                ViewModel.SelectionStatus = SelectionStatus.Hover;
                Cursor = new Cursor(StandardCursorType.Hand);
            }

            base.OnPointerEnter(e);
        }

        /// <inheritdoc />
        protected override void OnPointerLeave(PointerEventArgs e)
        {
            if (ViewModel?.SelectionStatus == SelectionStatus.Hover)
            {
                ViewModel.SelectionStatus = SelectionStatus.None;
                Cursor = new Cursor(StandardCursorType.Arrow);
            }

            base.OnPointerLeave(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}