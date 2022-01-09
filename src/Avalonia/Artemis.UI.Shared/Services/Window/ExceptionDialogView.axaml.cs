using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Shared.Services
{
    internal class ExceptionDialogView : ReactiveWindow<ExceptionDialogViewModel>
    {
        public ExceptionDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}