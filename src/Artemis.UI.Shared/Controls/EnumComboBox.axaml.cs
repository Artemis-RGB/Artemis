using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;

namespace Artemis.UI.Shared;

/// <summary>
///     Represents a combobox that can display the values of an enum.
/// </summary>
public partial class EnumComboBox : UserControl
{
    /// <summary>
    ///     Gets or sets the currently selected value
    /// </summary>
    public static readonly StyledProperty<object?> ValueProperty = AvaloniaProperty.Register<EnumComboBox, object?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    private readonly ObservableCollection<EnumComboBoxItem> _currentValues = new();
    private Type? _currentType;

    /// <summary>
    ///     Creates a new instance of the <see cref="EnumComboBox" /> class.
    /// </summary>
    public EnumComboBox()
    {
        PropertyChanged += OnPropertyChanged;
        InitializeComponent();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ValueProperty)
        {
            UpdateValues();
            UpdateSelection();
        }
    }

    /// <summary>
    ///     Gets or sets the currently selected value
    /// </summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ChildEnumComboBox == null || ChildEnumComboBox.SelectedIndex == -1)
            return;

        EnumComboBoxItem v = _currentValues[ChildEnumComboBox.SelectedIndex];
        if (!Equals(Value, v.Value))
            Value = v.Value;
    }

    private void UpdateValues()
    {
        Type? newType = Value?.GetType();
        if (ChildEnumComboBox == null || newType == null || _currentType == newType)
            return;

        _currentValues.Clear();
        foreach ((Enum, string) valueDesc in EnumUtilities.GetAllValuesAndDescriptions(newType))
            _currentValues.Add(new EnumComboBoxItem(value: valueDesc.Item1, description: valueDesc.Item2));

        _currentType = newType;
    }

    private void UpdateSelection()
    {
        if (ChildEnumComboBox == null || Value is not Enum)
            return;

        EnumComboBoxItem? value = _currentValues.FirstOrDefault(v => v.Value.Equals(Value));
        if (!Equals(value?.Value, ChildEnumComboBox.SelectedItem))
            ChildEnumComboBox.SelectedItem = value;
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ChildEnumComboBox.ItemsSource = _currentValues;

        UpdateValues();
        UpdateSelection();
        ChildEnumComboBox.SelectionChanged += OnSelectionChanged;

        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (ChildEnumComboBox != null)
            ChildEnumComboBox.SelectionChanged -= OnSelectionChanged;

        base.OnDetachedFromLogicalTree(e);
    }

    #endregion
}

/// <summary>
///     Represents an item in the <see cref="EnumComboBox" />
/// </summary>
public class EnumComboBoxItem
{
    /// <summary>
    ///     Creates a new instance of the <see cref="EnumComboBoxItem" /> class.
    /// </summary>
    public EnumComboBoxItem(Enum value, string description)
    {
        Value = value;
        Description = description;
    }

    /// <summary>
    ///     Gets or sets the value of the item
    /// </summary>
    public Enum Value { get; set; }
        
    /// <summary>
    ///     Gets or sets the description of the item
    /// </summary>
    public string Description { get; set; }
}