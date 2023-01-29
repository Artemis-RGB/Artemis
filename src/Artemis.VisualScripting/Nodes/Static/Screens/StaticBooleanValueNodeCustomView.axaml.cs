using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public partial class StaticBooleanValueNodeCustomView : ReactiveUserControl<StaticBooleanValueNodeCustomViewModel>
{
    public StaticBooleanValueNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}