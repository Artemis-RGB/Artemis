using Artemis.UI.Screens.Root;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.Core.ApplicationModel;

namespace Artemis.UI
{
    public class MainWindow : ReactiveCoreWindow<RootViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupTitlebar();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void SetupTitlebar()
        {
            object? titleBar = this.FindResource("RootWindowTitlebar");
            if (titleBar != null)
                SetTitleBar((IControl) titleBar);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}