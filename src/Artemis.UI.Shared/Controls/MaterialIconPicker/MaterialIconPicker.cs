using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
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

    private SourceList<MaterialIconKind>? _iconsSource;
    private TextBox? _searchBox;
    private IDisposable? _sub;
    private ItemsRepeater? _iconsContainer;
    private readonly ICommand _selectIcon;

    /// <inheritdoc />
    public MaterialIconPicker()
    {
        _selectIcon = ReactiveCommand.Create<MaterialIconKind>(i => Value = i);
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
    public static readonly DirectProperty<MaterialIconPicker, ICommand> SelectIconProperty =
        AvaloniaProperty.RegisterDirect<MaterialIconPicker, ICommand>(nameof(SelectIcon), g => g.SelectIcon);

    /// <summary>
    ///     Gets the command to execute when deleting stops.
    /// </summary>
    public ICommand SelectIcon
    {
        get => _selectIcon;
        private init => SetAndRaise(SelectIconProperty, ref _selectIcon, value);
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _searchBox = e.NameScope.Find<TextBox>("SearchBox");
        _iconsContainer = e.NameScope.Find<ItemsRepeater>("IconsContainer");
        if (_iconsContainer == null)
            return;

        _iconsSource = new SourceList<MaterialIconKind>();
        _iconsSource.AddRange(Enum.GetValues<MaterialIconKind>().Distinct());
        _sub = _iconsSource.Connect()
            .Filter(_searchBox.WhenAnyValue(s => s.Text).Throttle(TimeSpan.FromMilliseconds(100)).Select(CreatePredicate))
            .Sort(SortExpressionComparer<MaterialIconKind>.Descending(p => p.ToString()))
            .Bind(out ReadOnlyObservableCollection<MaterialIconKind> icons)
            .Subscribe();
        _iconsContainer.ItemsSource = icons;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _iconsSource?.Dispose();
        _iconsSource = null;
        _sub?.Dispose();
        _sub = null;
        base.OnDetachedFromLogicalTree(e);
    }

    private Func<MaterialIconKind, bool> CreatePredicate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _ => true;

        text = StripWhiteSpaceRegex().Replace(text, "");
        return data => data.ToString().Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex StripWhiteSpaceRegex();

    #endregion
}