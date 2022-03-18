using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class CableView : ReactiveUserControl<CableViewModel>
{
    public CableView()
    {
        InitializeComponent();
        Path cablePath = this.Get<Path>("CablePath");

        // Swap a margin on and off of the cable path to ensure the visual is always invalidated
        // This is a workaround for https://github.com/AvaloniaUI/Avalonia/issues/4748
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.FromPoint)
            .Subscribe(_ => cablePath.Margin = cablePath.Margin == new Thickness(0, 0, 0, 0) ? new Thickness(1, 1, 0, 0) : new Thickness(0, 0, 0, 0))
            .DisposeWith(d));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}