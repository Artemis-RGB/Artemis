using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class FloatPropertyInputView : ReactiveUserControl<FloatPropertyInputViewModel>
{
    public FloatPropertyInputView()
    {
        InitializeComponent();
        AddHandler(KeyUpEvent, OnRoutedKeyUp, handledEventsToo: true);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnRoutedKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Escape)
            FocusManager.Instance!.Focus(null);
    }
}