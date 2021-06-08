using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class SidebarCategoryCreateViewModel : DialogViewModelBase
    {
        private readonly IProfileService _profileService;
        private string _categoryName;

        public SidebarCategoryCreateViewModel(IProfileService profileService, IModelValidator<SidebarCategoryCreateViewModel> validator) : base(validator)
        {
            _profileService = profileService;
        }

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

            Session.Close(_profileService.CreateProfileCategory(CategoryName));
        }
    }

    public class SidebarCategoryCreateViewModelValidator : AbstractValidator<SidebarCategoryCreateViewModel>
    {
        private readonly IProfileCategoryRepository _profileCategoryRepository;

        public SidebarCategoryCreateViewModelValidator(IProfileCategoryRepository profileCategoryRepository)
        {
            _profileCategoryRepository = profileCategoryRepository;

            RuleFor(m => m.CategoryName)
                .NotEmpty().WithMessage("Category name may not be empty")
                .Must(BeUniqueCategory).WithMessage("Category name already taken");
        }

        private bool BeUniqueCategory(SidebarCategoryCreateViewModel viewModel, string categoryName)
        {
            return _profileCategoryRepository.IsUnique(categoryName.Trim(), null) == null;
        }
    }
}