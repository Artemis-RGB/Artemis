using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public partial class StaticNumericValueNodeCustomView : ReactiveUserControl<StaticNumericValueNodeCustomViewModel>
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