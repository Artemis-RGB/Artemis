using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;
using WinRT.Interop;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class SidebarCategoryUpdateViewModel : DialogViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IProfileService _profileService;
        private string _categoryName;

        public SidebarCategoryUpdateViewModel(ProfileCategory profileCategory, IProfileService profileService, IDialogService dialogService,
            IModelValidator<SidebarCategoryUpdateViewModel> validator) : base(validator)
        {
            _categoryName = profileCategory.Name;
            _profileService = profileService;
            _dialogService = dialogService;

            ProfileCategory = profileCategory;
        }

        public ProfileCategory ProfileCategory { get; }

        public string CategoryName
        {
            get => _categoryName;
            set => SetAndNotify(ref _categoryName, value);
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            ProfileCategory.Name = CategoryName;
            _profileService.SaveProfileCategory(ProfileCategory);
            
            Session.Close(nameof(Accept));
        }

        public void Delete()
        {
            Session.Close(nameof(Delete));
        }
    }

    public class SidebarCategoryUpdateViewModelValidator : AbstractValidator<SidebarCategoryUpdateViewModel>
    {
        private readonly IProfileCategoryRepository _profileCategoryRepository;

        public SidebarCategoryUpdateViewModelValidator(IProfileCategoryRepository profileCategoryRepository)
        {
            _profileCategoryRepository = profileCategoryRepository;

            RuleFor(m => m.CategoryName)
                .NotEmpty().WithMessage("Category name may not be empty")
                .Must(BeUniqueCategory).WithMessage("Category name already taken");
        }

        private bool BeUniqueCategory(SidebarCategoryUpdateViewModel viewModel, string categoryName)
        {
            return _profileCategoryRepository.IsUnique(categoryName, viewModel.ProfileCategory.EntityId) == null;
        }
    }
}