using System.Reactive;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Sidebar.ContentDialogs
{
    public class SidebarCategoryEditViewModel : ContentDialogViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly ProfileCategory? _category;
        private string? _categoryName;

        public SidebarCategoryEditViewModel(IProfileService profileService, ProfileCategory? category)
        {
            _profileService = profileService;
            _category = category;

            if (_category != null)
                _categoryName = _category.Name;

            Confirm = ReactiveCommand.Create(ExecuteConfirm, ValidationContext.Valid);
            Delete = ReactiveCommand.Create(ExecuteDelete);

            this.ValidationRule(vm => vm.CategoryName, categoryName => !string.IsNullOrWhiteSpace(categoryName), "You must specify a valid name");
        }

        public ReactiveCommand<Unit, Unit> Delete { get; set; }

        public string? CategoryName
        {
            get => _categoryName;
            set => this.RaiseAndSetIfChanged(ref _categoryName, value);
        }

        public ReactiveCommand<Unit, Unit> Confirm { get; }

        private void ExecuteConfirm()
        {
            if (_category != null)
            {
                _category.Name = CategoryName!;
                _profileService.SaveProfileCategory(_category);
            }
            else
                _profileService.CreateProfileCategory(CategoryName!);

            ContentDialog?.Hide(ContentDialogResult.Primary);
        }

        private void ExecuteDelete()
        {
            if (_category != null)
                _profileService.DeleteProfileCategory(_category);
        }
    }
}