using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Device;

public partial class DeviceLogicalLayoutDialogView : ReactiveUserControl<DeviceLogicalLayoutDialogViewModel>
{
    public DeviceLogicalLayoutDialogView()
    {
        InitializeComponent();
        
        RegionsAutoCompleteBox.ItemFilter += SearchRegions;
        Dispatcher.UIThread.InvokeAsync(DelayedAutoFocus);
    }

    private async Task DelayedAutoFocus()
    {
        await Task.Delay(200);
        RegionsAutoCompleteBox.Focus();
        RegionsAutoCompleteBox.PopulateComplete();
    }

    private bool SearchRegions(string? search, object? item)
    {
        if (search == null)
            return true;
        if (item is not RegionInfo regionInfo)
            return false;

        return regionInfo.EnglishName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
               regionInfo.NativeName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
               regionInfo.TwoLetterISORegionName.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

}