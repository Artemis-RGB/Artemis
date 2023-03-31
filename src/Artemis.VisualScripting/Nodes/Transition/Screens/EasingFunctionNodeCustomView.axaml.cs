using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Transition.Screens;

public partial class EasingFunctionNodeCustomView : ReactiveUserControl<EasingFunctionNodeCustomViewModel>
{
    public EasingFunctionNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}