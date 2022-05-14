using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Dialogs;

public class AddEffectViewModel : ContentDialogViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly RenderProfileElement _renderProfileElement;
    private string? _searchText;

    public AddEffectViewModel(RenderProfileElement renderProfileElement, IProfileEditorService profileEditorService, ILayerEffectService layerEffectService)
    {
        _renderProfileElement = renderProfileElement;
        _profileEditorService = profileEditorService;

        SourceList<LayerEffectDescriptor> layerEffectSourceList = new();
        layerEffectSourceList.AddRange(layerEffectService.GetLayerEffects());
        IObservable<Func<LayerEffectDescriptor, bool>> layerEffectFilter = this.WhenAnyValue(vm => vm.SearchText).Select(CreatePredicate);

        layerEffectSourceList.Connect()
            .Filter(layerEffectFilter)
            .Bind(out ReadOnlyObservableCollection<LayerEffectDescriptor> layerEffectDescriptors)
            .Subscribe();
        LayerEffectDescriptors = layerEffectDescriptors;
    }

    public ReadOnlyObservableCollection<LayerEffectDescriptor> LayerEffectDescriptors { get; }

    public string? SearchText
    {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public void AddLayerEffect(LayerEffectDescriptor descriptor)
    {
        BaseLayerEffect layerEffect = descriptor.CreateInstance(_renderProfileElement, null);
        _profileEditorService.ExecuteCommand(new AddLayerEffect(_renderProfileElement, layerEffect));
        ContentDialog?.Hide();
    }

    private Func<LayerEffectDescriptor, bool> CreatePredicate(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return _ => true;

        search = search.Trim();
        return data => data.DisplayName.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
                       data.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase);
    }
}