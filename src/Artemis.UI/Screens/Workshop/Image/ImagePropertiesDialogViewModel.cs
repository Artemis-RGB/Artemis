using System.Reactive;
using Artemis.UI.Shared;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Workshop.Image;

public partial class ImagePropertiesDialogViewModel : ContentDialogViewModelBase
{
    [Notify] private string? _name;
    [Notify] private string? _description;

    public ImagePropertiesDialogViewModel(string name, string description)
    {
        _name = string.IsNullOrWhiteSpace(name) ? null : name;
        _description =  string.IsNullOrWhiteSpace(description) ? null : description;
        Confirm = ReactiveCommand.Create(ExecuteConfirm, ValidationContext.Valid);

        this.ValidationRule(vm => vm.Name, input => !string.IsNullOrWhiteSpace(input), "Name is required");
        this.ValidationRule(vm => vm.Name, input => input?.Length <= 50, "Name can be a maximum of 50 characters");
        this.ValidationRule(vm => vm.Description, input => input == null || input.Length <= 150, "Description can be a maximum of 150 characters");
    }

    public ReactiveCommand<Unit, Unit> Confirm { get; }

    private void ExecuteConfirm()
    {
        if (!ValidationContext.IsValid)
            return;

        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}