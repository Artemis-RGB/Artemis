using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.CurrentUser;

public partial class WorkshopLoginView : ReactiveUserControl<WorkshopLoginViewModel>
{
    public WorkshopLoginView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}