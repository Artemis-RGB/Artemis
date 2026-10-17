using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Items;

public abstract class ConfigurationInputItemViewModel<TItem, T> : ActivatableViewModelBase where TItem : ConfigurationInputItem<T>
{
    protected ConfigurationInputItemViewModel(TItem item)
    {
        Item = item;

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => Item.PropertyChanged += h, h => Item.PropertyChanged -= h)
                .Select(e => e.EventArgs.PropertyName)
                .Where(p => p is nameof(ConfigurationInputItem<T>.Value) or nameof(ConfigurationInputItem<T>.DropdownValues))
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(HasDropdown));
                    this.RaisePropertyChanged(nameof(SelectedDropdownValue));
                })
                .DisposeWith(d);
        });
    }

    public TItem Item { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the user picks the value from a dropdown instead of entering it.
    /// </summary>
    public bool HasDropdown => Item.DropdownValues.Count > 0;

    public ConfigurationDropdownValue<T>? SelectedDropdownValue
    {
        get => Item.DropdownValues.FirstOrDefault(d => Equals(d.Value, Item.Value));
        set
        {
            if (value != null)
                Item.Value = value.Value;
        }
    }
}