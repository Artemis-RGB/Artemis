using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinCollectionView : ReactiveUserControl<PinCollectionViewModel>
{
    public InputPinCollectionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}