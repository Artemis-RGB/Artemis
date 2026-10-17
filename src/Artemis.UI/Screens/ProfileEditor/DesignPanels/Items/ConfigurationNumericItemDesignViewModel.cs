using System;
using System.ComponentModel;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public class ConfigurationNumericItemDesignViewModel : ConfigurationInputItemDesignViewModel<ConfigurationNumericItem, Numeric>
{
    public ConfigurationNumericItemDesignViewModel(ConfigurationNumericItem item, ConfigurationSection configurationSection) : base(item, configurationSection)
    {
        NumericItem = item;

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => item.PropertyChanged += h, h => item.PropertyChanged -= h)
                .Select(e => e.EventArgs.PropertyName)
                .Subscribe(p =>
                {
                    if (p == nameof(ConfigurationNumericItem.Minimum))
                        this.RaisePropertyChanged(nameof(Minimum));
                    else if (p == nameof(ConfigurationNumericItem.Maximum))
                        this.RaisePropertyChanged(nameof(Maximum));
                })
                .DisposeWith(d);
        });
    }

    public ConfigurationNumericItem NumericItem { get; }

    // A number box shows NaN as empty, use that to represent no minimum/maximum
    public double Minimum
    {
        get => NumericItem.Minimum ?? double.NaN;
        set => NumericItem.Minimum = double.IsNaN(value) ? null : value;
    }

    public double Maximum
    {
        get => NumericItem.Maximum ?? double.NaN;
        set => NumericItem.Maximum = double.IsNaN(value) ? null : value;
    }

    /// <inheritdoc />
    protected override Numeric CreateDropdownValue(int number)
    {
        return number;
    }
}