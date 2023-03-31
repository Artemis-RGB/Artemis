using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Mathematics.Screens;

public partial class MathExpressionNodeCustomView : ReactiveUserControl<MathExpressionNodeCustomViewModel>
{
    public MathExpressionNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        ViewModel?.UpdateInputValue();
    }
}