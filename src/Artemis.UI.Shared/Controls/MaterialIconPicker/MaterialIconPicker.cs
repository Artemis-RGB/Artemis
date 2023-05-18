using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Artemis.UI.Shared.Flyouts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.LogicalTree;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;

namespace Artemis.UI.Shared.MaterialIconPicker;

/// <summary>
///     Represents a Material icon picker picker that can be used to search and select a Material icon.
/// </summary>
public partial class MaterialIconPicker : TemplatedControl
{
    /// <summary>
    ///     Gets or sets the current Material icon.
    /// </summary>
    public static readonly StyledProperty<MaterialIconKind?> ValueProperty =
        AvaloniaProperty.Register<MaterialIconPicker, MaterialIconKind?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets the command to execute when deleting stops.
    /// </summary>
    public static readonly DirectProperty<MaterialIconPicker, ICommand> SelectIconProperty =
        AvaloniaProperty.RegisterDirect<MaterialIconPicker, ICommand>(nameof(SelectIcon), g => g.SelectIcon);

    private readonly ICommand _selectIcon;
    private ItemsRepeater? _iconsContainer;

    private SourceList<MaterialIconKind>? _iconsSource;
    private TextBox? _searchBox;
    private IDisposable? _sub;
    private readonly Dictionary<string,MaterialIconKind> _enumNames;

    /// <inheritdoc />
    public MaterialIconPicker()
    {
        _selectIcon = ReactiveCommand.Create<MaterialIconKind>(i =>
        {
            Value = i;
            Flyout?.Hide();
        });

        // Build a list of enum names and values, this is required because a value may have more than one name
        _enumNames = new Dictionary<string, MaterialIconKind>();
        MaterialIconKind[] values = Enum.GetValues<MaterialIconKind>();
        string[] names = Enum.GetNames<MaterialIconKind>();
        for (int index = 0; index < names.Length; index++)
            _enumNames[names[index]] = values[index];
    }

    /// <summary>
    ///     Gets or sets the current Material icon.
    /// </summary>
    public MaterialIconKind? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    ///     Gets the command to execute when deleting stops.
    /// </summary>
    public ICommand SelectIcon
    {
        get => _selectIcon;
        private init => SetAndRaise(SelectIconProperty, ref _selectIcon, value);
    }

    internal MaterialIconPickerFlyout? Flyout { get; set; }

    private void Setup()
    {
        if (_searchBox == null || _iconsContainer == null)
            return;

        // Build a list of values, they are not unique because a value with multiple names occurs once per name
        _iconsSource = new SourceList<MaterialIconKind>();
        _iconsSource.AddRange(Enum.GetValues<MaterialIconKind>().Distinct());
        _sub = _iconsSource.Connect()
            .Filter(_searchBox.WhenAnyValue(s => s.Text).Throttle(TimeSpan.FromMilliseconds(100)).Select(CreatePredicate))
            .Sort(SortExpressionComparer<MaterialIconKind>.Descending(p => p.ToString()))
            .Bind(out ReadOnlyObservableCollection<MaterialIconKind> icons)
            .Subscribe();
        _iconsContainer.ItemsSource = icons;
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _searchBox = e.NameScope.Find<TextBox>("SearchBox");
        _iconsContainer = e.NameScope.Find<ItemsRepeater>("IconsContainer");

        Setup();
        base.OnApplyTemplate(e);
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        Setup();
        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _iconsSource?.Dispose();
        _iconsSource = null;
        _sub?.Dispose();
        _sub = null;

        if (_searchBox != null)
            _searchBox.Text = "";
        base.OnDetachedFromLogicalTree(e);
    }

    private Func<MaterialIconKind, bool> CreatePredicate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _ => true;

        // Strip out whitespace and find all matching enum values
        text = StripWhiteSpaceRegex().Replace(text, "");
        HashSet<MaterialIconKind> values = _enumNames.Where(n => n.Key.Contains(text, StringComparison.OrdinalIgnoreCase)).Select(n => n.Value).ToHashSet();
        // Only show those that matched
        return data => values.Contains(data);
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex StripWhiteSpaceRegex();

    #endregion
}