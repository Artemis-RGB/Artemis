using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Artemis.Core.LayerBrushes;
using Artemis.UI.Shared;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree.Dialogs;

public class LayerBrushPresetViewModel : ContentDialogViewModelBase
{
    private readonly BaseLayerBrush _layerBrush;
    private string? _searchText;

    public LayerBrushPresetViewModel(BaseLayerBrush layerBrush)
    {
        _layerBrush = layerBrush;

        SourceList<ILayerBrushPreset> presetsSourceList = new();
        if (layerBrush.Presets != null)
            presetsSourceList.AddRange(layerBrush.Presets);
        IObservable<Func<ILayerBrushPreset, bool>> presetsFilter = this.WhenAnyValue(vm => vm.SearchText).Select(CreatePredicate);

        presetsSourceList.Connect()
            .Filter(presetsFilter)
            .Bind(out ReadOnlyObservableCollection<ILayerBrushPreset> presets)
            .Subscribe();
        Presets = presets;
    }

    public ReadOnlyObservableCollection<ILayerBrushPreset> Presets { get; }

    public string? SearchText
    {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public void SelectPreset(ILayerBrushPreset preset)
    {
        _layerBrush.BaseProperties?.ResetAllLayerProperties();
        preset.Apply();
        ContentDialog?.Hide();
    }

    private Func<ILayerBrushPreset, bool> CreatePredicate(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return _ => true;

        search = search.Trim();
        return data => data.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
                       data.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase);
    }
}