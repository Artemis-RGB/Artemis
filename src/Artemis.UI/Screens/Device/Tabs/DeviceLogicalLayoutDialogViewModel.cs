using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ContentDialogButton = Artemis.UI.Shared.Services.Builders.ContentDialogButton;

namespace Artemis.UI.Screens.Device;

public partial class DeviceLogicalLayoutDialogViewModel : ContentDialogViewModelBase
{
    private const int LOCALE_NEUTRAL = 0x0000;
    private const int LOCALE_CUSTOM_DEFAULT = 0x0c00;
    private const int LOCALE_INVARIANT = 0x007F;
    [Notify] private RegionInfo? _selectedRegion;

    public DeviceLogicalLayoutDialogViewModel(ArtemisDevice device)
    {
        Device = device;
        ApplyLogicalLayout = ReactiveCommand.Create(ExecuteApplyLogicalLayout, this.WhenAnyValue(vm => vm.SelectedRegion).Select(r => r != null));
        Regions = new ObservableCollection<RegionInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Where(c => c.LCID != LOCALE_INVARIANT &&
                        c.LCID != LOCALE_NEUTRAL &&
                        c.LCID != LOCALE_CUSTOM_DEFAULT &&
                        !c.CultureTypes.HasFlag(CultureTypes.UserCustomCulture))
            .Select(c => new RegionInfo(c.LCID))
            .GroupBy(r => r.EnglishName)
            .Select(g => g.First())
            .OrderBy(r => r.EnglishName));

        // Default to US/international
        SelectedRegion = Regions.FirstOrDefault(r => r.TwoLetterISORegionName == "US");
    }

    public ArtemisDevice Device { get; }
    public ReactiveCommand<Unit, Unit> ApplyLogicalLayout { get; }
    public ObservableCollection<RegionInfo> Regions { get; }
    public bool Applied { get; set; }

    public static async Task<bool> SelectLogicalLayout(IWindowService windowService, ArtemisDevice device)
    {
        await windowService.CreateContentDialog()
            .WithTitle("Select logical layout")
            .WithViewModel(out DeviceLogicalLayoutDialogViewModel vm, device)
            .WithCloseButtonText("Cancel")
            .WithDefaultButton(ContentDialogButton.Primary)
            .HavingPrimaryButton(b => b.WithText("Select").WithCommand(vm.ApplyLogicalLayout))
            .ShowAsync();

        return vm.Applied;
    }

    private void ExecuteApplyLogicalLayout()
    {
        if (SelectedRegion == null)
            return;

        Device.LogicalLayout = SelectedRegion.TwoLetterISORegionName;
        Applied = true;
        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}