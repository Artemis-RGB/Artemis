using Artemis.UI.Shared.Services.PropertyInput;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class EnumPropertyInputView : ReactiveUserControl<PropertyInputViewModel>
{
    public EnumPropertyInputView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}