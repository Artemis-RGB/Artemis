using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using FluentValidation;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileCreateViewModel : DialogViewModelBase
    {
        private readonly ProfileCategory _profileCategory;
        private readonly IProfileService _profileService;
        private string _profileName;
        private IconViewModel _selectedIcon;
        private bool _initializing;

        public ProfileCreateViewModel(ProfileCategory profileCategory, IProfileService profileService, IModelValidator<ProfileCreateViewModel> validator) : base(validator)
        {
            _profileCategory = profileCategory;
            _profileService = profileService;

            Icons = new BindableCollection<IconViewModel>();
            Initializing = true;
            Task.Run(() =>
            {
                Icons.AddRange(Enum.GetValues<PackIconKind>()
                    .GroupBy(e => e)
                    .Select(g => g.First())
                    .Select(e => new IconViewModel(e))
                    .ToList());
                SelectedIcon = Icons.FirstOrDefault();
                Initializing = false;
            });
        }

        public bool Initializing
        {
            get => _initializing;
            set => SetAndNotify(ref _initializing, value);
        }

        public string ProfileName
        {
            get => _profileName;
            set => SetAndNotify(ref _profileName, value);
        }

        public BindableCollection<IconViewModel> Icons { get; }

        public IconViewModel SelectedIcon
        {
            get => _selectedIcon;
            set => SetAndNotify(ref _selectedIcon, value);
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            _profileService.AddProfileConfiguration(_profileCategory, ProfileName, SelectedIcon.Icon.ToString());
            _profileService.UpdateProfileCategory(_profileCategory);
            Session.Close();
        }
    }

    public class ProfileCreateViewModelValidator : AbstractValidator<ProfileCreateViewModel>
    {
        public ProfileCreateViewModelValidator()
        {
            RuleFor(m => m.ProfileName).NotEmpty().WithMessage("Profile name may not be empty");
        }
    }
}