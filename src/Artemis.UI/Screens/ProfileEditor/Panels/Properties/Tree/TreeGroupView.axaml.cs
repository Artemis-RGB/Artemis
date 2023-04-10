using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree;

public partial class TreeGroupView : ReactiveUserControl<TreeGroupViewModel>
{
    public TreeGroupView()
    {
        InitializeComponent();
    }


    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel != null)
            ViewModel.PropertyGroupViewModel.IsExpanded = !ViewModel.PropertyGroupViewModel.IsExpanded;
    }
}