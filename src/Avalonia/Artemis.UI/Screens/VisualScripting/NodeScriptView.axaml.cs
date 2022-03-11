using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting
{
    public partial class NodeScriptView : ReactiveUserControl<NodeScriptViewModel>
    {
        public NodeScriptView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
                ViewModel?.ShowNodePicker(e.GetCurrentPoint(this).Position);
            if (e.InitialPressMouseButton == MouseButton.Left)
                ViewModel?.HideNodePicker();
        }
    }
}
