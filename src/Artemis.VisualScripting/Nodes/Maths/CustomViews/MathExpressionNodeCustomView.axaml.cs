using Artemis.VisualScripting.Nodes.Maths.CustomViewModels;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Maths.CustomViews;

public class MathExpressionNodeCustomView : ReactiveUserControl<MathExpressionNodeCustomViewModel>
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