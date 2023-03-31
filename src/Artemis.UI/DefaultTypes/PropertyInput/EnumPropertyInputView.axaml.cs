using Artemis.UI.Shared.Services.PropertyInput;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public partial class EnumPropertyInputView : ReactiveUserControl<PropertyInputViewModel>
{
    public EnumPropertyInputView()
    {
        InitializeComponent();
    }

}