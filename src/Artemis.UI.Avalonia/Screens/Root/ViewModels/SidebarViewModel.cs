using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Ninject.Factories;
using Artemis.UI.Avalonia.Screens.Home.ViewModels;
using Artemis.UI.Avalonia.Screens.Settings.ViewModels;
using Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels;
using Artemis.UI.Avalonia.Screens.Workshop.ViewModels;
using Material.Icons;
using Ninject;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
{
    public class SidebarViewModel : ViewModelBase
    {
        private readonly IKernel _kernel;
        private readonly IProfileService _profileService;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private SidebarScreenViewModel _selectedSidebarScreen;

        public SidebarViewModel(IKernel kernel, IProfileService profileService, ISidebarVmFactory sidebarVmFactory)
        {
            _kernel = kernel;
            _profileService = profileService;
            _sidebarVmFactory = sidebarVmFactory;

            SidebarScreens = new ObservableCollection<SidebarScreenViewModel>
            {
                new SidebarScreenViewModel<HomeViewModel>(MaterialIconKind.Home, "Home"),
                new SidebarScreenViewModel<WorkshopViewModel>(MaterialIconKind.TestTube, "Workshop"),
                new SidebarScreenViewModel<SurfaceEditorViewModel>(MaterialIconKind.Devices, "Surface Editor"),
                new SidebarScreenViewModel<SettingsViewModel>(MaterialIconKind.Cog, "Settings")
            };
            SelectedSidebarScreen = SidebarScreens.First();
            UpdateProfileCategories();
        }

        public ObservableCollection<SidebarScreenViewModel> SidebarScreens { get; }
        public ObservableCollection<SidebarCategoryViewModel> SidebarCategories { get; } = new();
        
        public SidebarScreenViewModel SelectedSidebarScreen
        {
            get => _selectedSidebarScreen;
            set => this.RaiseAndSetIfChanged(ref _selectedSidebarScreen, value);
        }

        private void UpdateProfileCategories()
        {
            SidebarCategories.Clear();
            foreach (ProfileCategory profileCategory in _profileService.ProfileCategories.OrderBy(p => p.Order))
                AddProfileCategoryViewModel(profileCategory);
        }

        public SidebarCategoryViewModel AddProfileCategoryViewModel(ProfileCategory profileCategory)
        {
            SidebarCategoryViewModel viewModel = _sidebarVmFactory.SidebarCategoryViewModel(profileCategory);
            SidebarCategories.Add(viewModel);
            return viewModel;
        }
    }
}