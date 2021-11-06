using Avalonia.Markup.Xaml;

namespace Artemis.UI.Avalonia.Shared.Services
{
    public partial class ExceptionDialogView : ReactiveWindow<ExceptionDialogViewModel>
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
