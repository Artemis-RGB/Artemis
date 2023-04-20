using System.Linq;
using System.Reactive;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Sidebar;

public class SidebarCategoryEditViewModel : ContentDialogViewModelBase
{
    private readonly ProfileCategory? _category;
    private readonly IProfileService _profileService;
    private string? _categoryName;

    public SidebarCategoryEditViewModel(IProfileService profileService, ProfileCategory category)
    {
        _profileService = profileService;
        _category = category == ProfileCategory.Empty ? null : category;

        if (_category != null)
            _categoryName = _category.Name;

        Confirm = ReactiveCommand.Create(ExecuteConfirm, ValidationContext.Valid);
        Enter = ReactiveCommand.Create(() => ContentDialog?.Hide(ContentDialogResult.Primary), Confirm.CanExecute);
        this.ValidationRule(vm => vm.CategoryName, categoryName => !string.IsNullOrWhiteSpace(categoryName?.Trim()), "You must specify a valid name");
        this.ValidationRule(vm => vm.CategoryName, categoryName => profileService.ProfileCategories.All(c => c.Name != categoryName?.Trim()), "You must specify a unique name");
    }

    public string? CategoryName
    {
        get => _categoryName;
        set => RaiseAndSetIfChanged(ref _categoryName, value);
    }

    public ReactiveCommand<Unit, Unit> Confirm { get; }
    public ReactiveCommand<Unit, Unit> Enter { get; }

    private void ExecuteConfirm()
    {
        if (_category != null)
        {
            _category.Name = CategoryName!.Trim();
            _profileService.SaveProfileCategory(_category);
        }
        else
        {
            _profileService.CreateProfileCategory(CategoryName!.Trim());
        }

        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}