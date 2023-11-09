using System.Reactive;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Workshop.Image;

public partial class ImagePropertiesDialogViewModel : ContentDialogViewModelBase
{
    private readonly ImageUploadRequest _image;
    [Notify] private string? _name;
    [Notify] private string? _description;

    public ImagePropertiesDialogViewModel(ImageUploadRequest image)
    {
        _image = image;
        _name = image.Name;
        _description = image.Description;

        Confirm = ReactiveCommand.Create(ExecuteConfirm, ValidationContext.Valid);

        this.ValidationRule(vm => vm.Name, input => !string.IsNullOrWhiteSpace(input), "Name is required");
        this.ValidationRule(vm => vm.Name, input => input?.Length <= 50, "Name can be a maximum of 50 characters");
        this.ValidationRule(vm => vm.Description, input => input?.Length <= 150, "Description can be a maximum of 150 characters");
    }

    public ReactiveCommand<Unit, Unit> Confirm { get; }

    private void ExecuteConfirm()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return;

        _image.Name = Name;
        _image.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;

        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}