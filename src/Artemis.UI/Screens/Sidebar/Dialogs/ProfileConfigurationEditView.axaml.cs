using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class ProfileConfigurationEditView : ReactiveAppWindow<ProfileConfigurationEditViewModel>
{
    public ProfileConfigurationEditView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.SelectedBitmapSource).Subscribe(_ => FillPreview.InvalidateVisual()).DisposeWith(d));

#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}