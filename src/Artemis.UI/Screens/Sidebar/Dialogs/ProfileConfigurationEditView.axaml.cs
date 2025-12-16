using System;
using System.Reactive.Disposables.Fluent;
using Artemis.UI.Shared;
using Avalonia;
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

}