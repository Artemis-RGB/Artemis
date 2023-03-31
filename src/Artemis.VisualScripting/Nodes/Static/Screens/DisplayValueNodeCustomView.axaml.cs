using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public partial class DisplayValueNodeCustomView : ReactiveUserControl<DisplayValueNodeCustomViewModel>
{
    public DisplayValueNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}