using Artemis.Core;
using Artemis.UI.Shared;
using FluentAvalonia.UI.Controls;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Section;

public class AddConfigurationItemDesignViewModel : ContentDialogViewModelBase
{
    public AddConfigurationItemDesignViewModel(ConfigurationSection section)
    {
        Section = section;
    }

    public ConfigurationSection Section { get; }

    public void AddItem(IConfigurationItem item)
    {
        Section.Items.Add(item);
        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}