using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables.Fluent;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Items;
using Artemis.UI.Shared;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public partial class ConfigurationSectionViewModel : ActivatableViewModelBase
{
    [Notify] private ReadOnlyObservableCollection<ActivatableViewModelBase>? _items;

    public ConfigurationSectionViewModel(ConfigurationSection configurationSection)
    {
        ConfigurationSection = configurationSection;

        this.WhenActivated(d =>
        {
            ConfigurationSection.Items
                .ToObservableChangeSet()
                .Transform(CreateViewModelForItem)
                .Bind(out ReadOnlyObservableCollection<ActivatableViewModelBase> items)
                .Subscribe()
                .DisposeWith(d);

            Items = items;
        });
    }

    public ConfigurationSection ConfigurationSection { get; }

    private static ActivatableViewModelBase CreateViewModelForItem(IConfigurationItem item)
    {
        return item switch
        {
            ConfigurationTextItem textItem => new ConfigurationTextItemViewModel(textItem),
            ConfigurationStringItem stringItem => new ConfigurationStringItemViewModel(stringItem),
            ConfigurationBooleanItem booleanItem => new ConfigurationBooleanItemViewModel(booleanItem),
            ConfigurationNumericItem numberItem => new ConfigurationNumericItemViewModel(numberItem),
            ConfigurationSKColorItem colorItem => new ConfigurationSKColorItemViewModel(colorItem),
            ConfigurationColorGradientItem colorGradientItem => new ConfigurationColorGradientItemViewModel(colorGradientItem),
            _ => throw new ArgumentException($"Unknown configuration item type: {item.GetType()}")
        };
    }
}