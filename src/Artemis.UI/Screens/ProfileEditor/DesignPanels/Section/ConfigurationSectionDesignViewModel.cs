using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Section;

public partial class ConfigurationSectionDesignViewModel : ActivatableViewModelBase
{
    private readonly IWindowService _windowService;
    [Notify] private ReadOnlyObservableCollection<ConfigurationItemDesignViewModel>? _items;

    public ConfigurationSectionDesignViewModel(ConfigurationSection configurationSection, IWindowService windowService)
    {
        _windowService = windowService;
        ConfigurationSection = configurationSection;

        this.WhenActivated(d =>
        {
            ConfigurationSection.Items
                .ToObservableChangeSet()
                .Transform(CreateViewModelForItem)
                .Bind(out ReadOnlyObservableCollection<ConfigurationItemDesignViewModel> items)
                .Subscribe()
                .DisposeWith(d);

            Items = items;
        });
    }

    public ConfigurationSection ConfigurationSection { get; }

    public void RemoveSection()
    {
        ConfigurationSection.ProfileConfiguration.ConfigurationSections.Remove(ConfigurationSection);
    }

    public async Task AddItem()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Add Item")
            .WithViewModel(out AddConfigurationItemDesignViewModel _, ConfigurationSection)
            .ShowAsync();
    }

    public void RemoveItem(ConfigurationItemDesignViewModel viewModel)
    {
        ConfigurationSection.Items.Remove(viewModel.Item);
    }

    private ConfigurationItemDesignViewModel CreateViewModelForItem(IConfigurationItem item)
    {
        return item switch
        {
            ConfigurationTextItem textItem => new ConfigurationTextItemDesignViewModel(textItem, ConfigurationSection),
            ConfigurationStringItem stringItem => new ConfigurationStringItemDesignViewModel(stringItem, ConfigurationSection),
            ConfigurationBooleanItem booleanItem => new ConfigurationBooleanItemDesignViewModel(booleanItem, ConfigurationSection),
            ConfigurationNumericItem numberItem => new ConfigurationNumericItemDesignViewModel(numberItem, ConfigurationSection),
            ConfigurationSKColorItem colorItem => new ConfigurationSKColorItemDesignViewModel(colorItem, ConfigurationSection),
            ConfigurationColorGradientItem colorGradientItem => new ConfigurationColorGradientItemDesignViewModel(colorGradientItem, ConfigurationSection),
            _ => throw new ArgumentException($"Unknown configuration item type: {item.GetType()}")
        };
    }
}