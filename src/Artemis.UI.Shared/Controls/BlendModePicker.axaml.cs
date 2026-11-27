using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;

namespace Artemis.UI.Shared;

/// <summary>
///     Represents a picker for blend modes that explains each blend mode and groups them by what they do.
/// </summary>
public partial class BlendModePicker : UserControl
{
    /// <summary>
    ///     Gets or sets the selected blend mode.
    /// </summary>
    public static readonly StyledProperty<FolderBlendMode> ValueProperty =
        AvaloniaProperty.Register<BlendModePicker, FolderBlendMode>(nameof(Value), FolderBlendMode.SrcOver, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets a value indicating whether <see cref="FolderBlendMode.PassThrough" /> can be picked.
    /// </summary>
    public static readonly StyledProperty<bool> ShowPassThroughProperty = AvaloniaProperty.Register<BlendModePicker, bool>(nameof(ShowPassThrough));

    private List<BlendModeOption> _options = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlendModePicker" /> class.
    /// </summary>
    public BlendModePicker()
    {
        InitializeComponent();
        UpdateOptions();
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    ///     Gets or sets the selected blend mode.
    /// </summary>
    public FolderBlendMode Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether <see cref="FolderBlendMode.PassThrough" /> can be picked.
    /// </summary>
    public bool ShowPassThrough
    {
        get => GetValue(ShowPassThroughProperty);
        set => SetValue(ShowPassThroughProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        UpdateSelection();
        ChildComboBox.SelectionChanged += OnSelectionChanged;
        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ChildComboBox.SelectionChanged -= OnSelectionChanged;
        base.OnDetachedFromLogicalTree(e);
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ShowPassThroughProperty)
            UpdateOptions();
        if (e.Property == ValueProperty || e.Property == ShowPassThroughProperty)
            UpdateSelection();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ChildComboBox.SelectedItem is BlendModeOption {Value: { } value} && value != Value)
            Value = value;
    }

    private void UpdateOptions()
    {
        _options = BlendModeOption.All.Where(o => ShowPassThrough || o.Value != FolderBlendMode.PassThrough).ToList();
        ChildComboBox.ItemsSource = _options;
    }

    private void UpdateSelection()
    {
        BlendModeOption? selected = _options.FirstOrDefault(o => o.Value == Value);
        if (!ReferenceEquals(ChildComboBox.SelectedItem, selected))
            ChildComboBox.SelectedItem = selected;
    }
}
