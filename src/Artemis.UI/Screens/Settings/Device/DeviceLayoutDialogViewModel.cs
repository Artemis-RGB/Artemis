using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using MaterialDesignExtensions.Model;

namespace Artemis.UI.Screens.Settings.Device
{
    public class DeviceLayoutDialogViewModel : DialogViewModelBase
    {
        private readonly IRgbService _rgbService;
        private bool _selectPhysicalLayout;
        private RegionInfoAutocompleteSource _autocompleteSource;
        private RegionInfo _selectedRegion;

        public DeviceLayoutDialogViewModel(ArtemisDevice device, IRgbService rgbService)
        {
            _rgbService = rgbService;
            Device = device;
            SelectPhysicalLayout = !device.DeviceProvider.CanDetectPhysicalLayout;

            Task.Run(() => AutocompleteSource = new RegionInfoAutocompleteSource());
        }

        public ArtemisDevice Device { get; }

        public RegionInfoAutocompleteSource AutocompleteSource
        {
            get => _autocompleteSource;
            set => SetAndNotify(ref _autocompleteSource, value);
        }

        public RegionInfo SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                SetAndNotify(ref _selectedRegion, value);
                NotifyOfPropertyChange(nameof(CanConfirm));
            }
        }

        public bool SelectPhysicalLayout
        {
            get => _selectPhysicalLayout;
            set => SetAndNotify(ref _selectPhysicalLayout, value);
        }

        public bool CanConfirm => SelectedRegion != null;

        public void ApplyPhysicalLayout(string physicalLayout)
        {
            Device.PhysicalLayout = Enum.Parse<KeyboardLayoutType>(physicalLayout);

            _rgbService.SaveDevice(Device);
            _rgbService.ApplyBestDeviceLayout(Device);

            SelectPhysicalLayout = false;
        }

        private void ApplyLogicalLayout(string logicalLayout)
        {
            Device.LogicalLayout = logicalLayout;

            _rgbService.SaveDevice(Device);
            _rgbService.ApplyBestDeviceLayout(Device);
        }

        public void Confirm()
        {
            if (!CanConfirm || Session == null || Session.IsEnded)
                return;

            ApplyLogicalLayout(SelectedRegion.TwoLetterISORegionName);
            Session?.Close(true);
        }
    }

    public class RegionInfoAutocompleteSource : IAutocompleteSource<RegionInfo>
    {
        public List<RegionInfo> Regions { get; set; }

        public RegionInfoAutocompleteSource()
        {
            Regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList()
                .Select(c => new RegionInfo(c.LCID))
                .GroupBy(r => r.EnglishName)
                .Select(g => g.First())
                .OrderBy(r => r.EnglishName)
                .ToList();
        }

        IEnumerable<RegionInfo> IAutocompleteSource<RegionInfo>.Search(string searchTerm)
        {
            if (searchTerm == null)
                return Regions;

            searchTerm = searchTerm.ToLower();
            return Regions.Where(r => r.EnglishName.ToLower().Contains(searchTerm) ||
                                      r.NativeName.ToLower().Contains(searchTerm) ||
                                      r.TwoLetterISORegionName.ToLower().Contains(searchTerm));
        }

        public IEnumerable Search(string searchTerm)
        {
            if (searchTerm == null)
                return Regions;

            searchTerm = searchTerm.ToLower();
            return Regions.Where(r => r.EnglishName.ToLower().Contains(searchTerm) ||
                                      r.NativeName.ToLower().Contains(searchTerm) ||
                                      r.TwoLetterISORegionName.ToLower().Contains(searchTerm));
        }
    }
}