using Artemis.Core.LayerEffects;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Dialogs;

public partial class AddEffectView : ReactiveUserControl<AddEffectViewModel>
{
    public AddEffectView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not IDataContextProvider {DataContext: LayerEffectDescriptor descriptor} || ViewModel == null)
            return;

        ViewModel?.AddLayerEffect(descriptor);
    }
}