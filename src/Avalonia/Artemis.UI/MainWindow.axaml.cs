using Artemis.UI.Screens.Root;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
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
            ICoreApplicationView coreAppTitleBar = this;
            if (coreAppTitleBar.TitleBar != null)
            {
                coreAppTitleBar.TitleBar.ExtendViewIntoTitleBar = true;
                SetTitleBar(this.Get<Border>("TitleBar"));
            }
  
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}