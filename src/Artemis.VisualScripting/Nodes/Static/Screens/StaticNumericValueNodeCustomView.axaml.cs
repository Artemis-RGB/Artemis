using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public class StaticNumericValueNodeCustomView : ReactiveUserControl<StaticNumericValueNodeCustomViewModel>
{
    public StaticNumericValueNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}