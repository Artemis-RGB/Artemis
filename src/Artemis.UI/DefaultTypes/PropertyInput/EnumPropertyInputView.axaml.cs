using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI.Avalonia;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public partial class EnumPropertyInputView : ReactiveUserControl<PropertyInputViewModel>
{
    public EnumPropertyInputView()
    {
        InitializeComponent();
    }

}