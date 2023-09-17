using System.Reactive;
using Artemis.Core.LayerEffects;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree.ContentDialogs;

public class LayerEffectRenameViewModel : ContentDialogViewModelBase
{
    private readonly BaseLayerEffect _layerEffect;
    private readonly IProfileEditorService _profileEditorService;
    private string? _layerEffectName;

    public LayerEffectRenameViewModel(IProfileEditorService profileEditorService, BaseLayerEffect layerEffect)
    {
        _profileEditorService = profileEditorService;
        _layerEffect = layerEffect;
        _layerEffectName = layerEffect.Name;

        Confirm = ReactiveCommand.Create(ExecuteConfirm, ValidationContext.Valid);
        this.ValidationRule(vm => vm.LayerEffectName, categoryName => !string.IsNullOrWhiteSpace(categoryName), "You must specify a valid name");
    }

    public string? LayerEffectName
    {
        get => _layerEffectName;
        set => RaiseAndSetIfChanged(ref _layerEffectName, value);
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