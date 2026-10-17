using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DesignPanels.Items;

public abstract class ConfigurationInputItemDesignViewModel<TItem, T> : ConfigurationItemDesignViewModel where TItem : ConfigurationInputItem<T>
{
    protected ConfigurationInputItemDesignViewModel(TItem item, ConfigurationSection configurationSection) : base(item, configurationSection)
    {
        InputItem = item;
        RemoveDropdownValue = ReactiveCommand.Create<ConfigurationDropdownValue<T>>(ExecuteRemoveDropdownValue);

        this.WhenActivated(d =>
        {
            IObservable<string?> propertyChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => InputItem.PropertyChanged += h,
                    h => InputItem.PropertyChanged -= h)
                .Select(e => e.EventArgs.PropertyName);

            // While designing, the value follows the default so that the preview shows what users will initially get
            propertyChanged.Where(p => p == nameof(ConfigurationInputItem<T>.DefaultValue))
                .Subscribe(_ =>
                {
                    InputItem.ResetToDefault();
                    this.RaisePropertyChanged(nameof(SelectedDefaultDropdownValue));
                })
                .DisposeWith(d);

            propertyChanged.Where(p => p == nameof(ConfigurationInputItem<T>.DropdownValues))
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(HasDropdown));
                    this.RaisePropertyChanged(nameof(SelectedDefaultDropdownValue));
                })
                .DisposeWith(d);
        });
    }

    public TItem InputItem { get; }

    /// <summary>
    ///     Gets a boolean indicating whether users pick the value from a dropdown instead of entering it.
    /// </summary>
    public bool HasDropdown => InputItem.DropdownValues.Count > 0;

    public ConfigurationDropdownValue<T>? SelectedDefaultDropdownValue
    {
        get => InputItem.DropdownValues.FirstOrDefault(d => Equals(d.Value, InputItem.DefaultValue));
        set
        {
            if (value != null)
                InputItem.DefaultValue = value.Value;
        }
    }

    public ReactiveCommand<ConfigurationDropdownValue<T>, Unit> RemoveDropdownValue { get; }

    public void AddDropdownValue()
    {
        int number = InputItem.DropdownValues.Count + 1;
        ConfigurationDropdownValue<T> dropdownValue = new() {DisplayName = $"Option {number}", Value = CreateDropdownValue(number)};
        InputItem.DropdownValues.Add(dropdownValue);

        // The default should always be one of the options
        if (SelectedDefaultDropdownValue == null)
            InputItem.DefaultValue = dropdownValue.Value;
    }

    /// <summary>
    ///     Creates the value of a newly added dropdown option.
    /// </summary>
    /// <param name="number">The number of the new option, starting at 1.</param>
    protected virtual T CreateDropdownValue(int number)
    {
        throw new NotSupportedException($"{GetType().Name} does not support dropdown values");
    }

    private void ExecuteRemoveDropdownValue(ConfigurationDropdownValue<T> dropdownValue)
    {
        InputItem.DropdownValues.Remove(dropdownValue);
        if (HasDropdown && SelectedDefaultDropdownValue == null)
            InputItem.DefaultValue = InputItem.DropdownValues[0].Value;
    }
}