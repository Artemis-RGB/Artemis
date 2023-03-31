using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

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

    private readonly ObservableCollection<(Enum, string)> _currentValues = new();
    private Type? _currentType;

    private ComboBox? _enumComboBox;

    /// <summary>
    ///     Creates a new instance of the <see cref="EnumComboBox" /> class.
    /// </summary>
    public EnumComboBox()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Gets or sets the currently selected value
    /// </summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set
        {
            SetValue(ValueProperty, value);
            UpdateValues();
            UpdateSelection();
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_enumComboBox == null || _enumComboBox.SelectedIndex == -1)
            return;

        (Enum enumValue, _) = _currentValues[_enumComboBox.SelectedIndex];
        if (!Equals(Value, enumValue))
            Value = enumValue;
    }

    private void UpdateValues()
    {
        Type? newType = Value?.GetType();
        if (_enumComboBox == null || newType == null || _currentType == newType)
            return;

        _currentValues.Clear();
        foreach ((Enum, string) valueDesc in EnumUtilities.GetAllValuesAndDescriptions(newType))
            _currentValues.Add(valueDesc);

        _currentType = newType;
    }

    private void UpdateSelection()
    {
        if (_enumComboBox == null || Value is not Enum)
            return;

        (Enum, string) value = _currentValues.FirstOrDefault(v => v.Item1.Equals(Value));
        if (!Equals(value.Item1, _enumComboBox.SelectedItem))
            _enumComboBox.SelectedItem = value;
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _enumComboBox = this.Get<ComboBox>("ChildEnumComboBox");
        _enumComboBox.Items = _currentValues;

        UpdateValues();
        UpdateSelection();
        _enumComboBox.SelectionChanged += OnSelectionChanged;

        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_enumComboBox != null)
            _enumComboBox.SelectionChanged -= OnSelectionChanged;

        base.OnDetachedFromLogicalTree(e);
    }

    #endregion
}