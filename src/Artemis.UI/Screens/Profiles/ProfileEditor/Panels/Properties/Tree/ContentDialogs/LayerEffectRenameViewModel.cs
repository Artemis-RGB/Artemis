using System.Reactive;
using Artemis.Core.LayerEffects;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Tree.ContentDialogs;

public partial class LayerEffectRenameViewModel : ContentDialogViewModelBase
{
    private readonly BaseLayerEffect _layerEffect;
    private readonly IProfileEditorService _profileEditorService;
    [Notify] private string? _layerEffectName;

    public LayerEffectRenameViewModel(IProfileEditorService profileEditorService, BaseLayerEffect layerEffect)
    {
        _profileEditorService = profileEditorService;
        _layerEffect = layerEffect;
        _layerEffectName = layerEffect.Name;

        Confirm = ReactiveCommand.Create(ExecuteConfirm, ValidationContext.Valid);
        this.ValidationRule<LayerEffectRenameViewModel, string>(vm => vm.LayerEffectName, categoryName => !string.IsNullOrWhiteSpace(categoryName), "You must specify a valid name");
    }

    public ReactiveCommand<Unit, Unit> Confirm { get; }

    private void ExecuteConfirm()
    {
        if (LayerEffectName == null)
            return;

        _profileEditorService.ExecuteCommand(new RenameLayerEffect(_layerEffect, LayerEffectName));
        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}