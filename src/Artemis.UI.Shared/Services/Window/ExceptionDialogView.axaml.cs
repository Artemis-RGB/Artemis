using Avalonia;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Shared.Services;

internal partial class ExceptionDialogView : ReactiveWindow<ExceptionDialogViewModel>
{
    public ExceptionDialogView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

}