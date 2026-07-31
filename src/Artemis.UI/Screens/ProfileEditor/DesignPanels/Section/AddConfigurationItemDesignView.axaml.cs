using Artemis.Core;
using Avalonia.Controls;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Section;

public partial class AddConfigurationItemDesignView : ReactiveUserControl<AddConfigurationItemDesignViewModel>
{
    public AddConfigurationItemDesignView()
    {
        InitializeComponent();
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems[0] is not ListBoxItem item) return;

        if (item == BooleanInputItem)
            ViewModel?.AddItem(new ConfigurationBooleanItem {Name = "New Boolean"});
        else if (item == ColorGradientInputItem)
            ViewModel?.AddItem(new ConfigurationColorGradientItem {Name = "New Gradient"});
        else if (item == ColorInputItem)
            ViewModel?.AddItem(new ConfigurationSKColorItem {Name = "New Color"});
        else if (item == NumericInputItem)
            ViewModel?.AddItem(new ConfigurationNumericItem {Name = "New Numeric"});
        else if (item == TextInputItem)
            ViewModel?.AddItem(new ConfigurationStringItem {Name = "New String"});
        else if (item == TextBlockItem)
            ViewModel?.AddItem(new ConfigurationTextItem {Text = "New Text"});
    }
}