using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public partial class BoolPropertyInputView : ReactiveUserControl<BoolPropertyInputViewModel>
{
    public BoolPropertyInputView()
    {
        InitializeComponent();
    }

}