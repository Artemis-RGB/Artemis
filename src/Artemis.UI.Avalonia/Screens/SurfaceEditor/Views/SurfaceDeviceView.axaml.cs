using Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.Views
{
    public class SurfaceDeviceView : ReactiveUserControl<SurfaceDeviceViewModel>
    {
        public SurfaceDeviceView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Overrides of InputElement

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

        /// <inheritdoc />
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && ViewModel != null) 
                ViewModel.SelectionStatus = SelectionStatus.Selected;

            base.OnPointerPressed(e);
        }

        #endregion
    }
}