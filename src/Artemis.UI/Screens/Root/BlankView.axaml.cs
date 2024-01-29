using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Root;

public partial class BlankView : ReactiveUserControl<BlankViewModel>
{
    public BlankView()
    {
        InitializeComponent();
    }
}