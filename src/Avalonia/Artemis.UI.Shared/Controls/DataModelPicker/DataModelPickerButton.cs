using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Controls.Flyouts;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentAvalonia.Core;

namespace Artemis.UI.Shared.Controls.DataModelPicker;

/// <summary>
///     Represents a button that can be used to pick a data model path in a flyout.
/// </summary>
public class DataModelPickerButton : TemplatedControl
{
    /// <summary>
    ///     Gets or sets the placeholder to show when nothing is selected.
    /// </summary>
    public static readonly StyledProperty<string> PlaceholderProperty =
        AvaloniaProperty.Register<DataModelPicker, string>(nameof(Placeholder), "Click to select");

    /// <summary>
    ///     Gets or sets a boolean indicating whether the data model picker should show the full path of the selected value.
    /// </summary>
    public static readonly StyledProperty<bool> ShowFullPathProperty =
        AvaloniaProperty.Register<DataModelPicker, bool>(nameof(ShowFullPath));

    /// <summary>
    ///     Gets a boolean indicating whether the data model picker has a value.
    /// </summary>
    public static readonly StyledProperty<bool> HasValueProperty =
        AvaloniaProperty.Register<DataModelPicker, bool>(nameof(HasValue));

    /// <summary>
    /// Gets or sets the desired flyout placement.
    /// </summary>
    public static readonly StyledProperty<FlyoutPlacementMode> PlacementProperty =
        AvaloniaProperty.Register<FlyoutBase, FlyoutPlacementMode>(nameof(Placement));

    /// <summary>
    ///     Gets or sets data model path.
    /// </summary>
    public static readonly StyledProperty<DataModelPath?> DataModelPathProperty =
        AvaloniaProperty.Register<DataModelPicker, DataModelPath?>(nameof(DataModelPath), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets a boolean indicating whether the data model picker should show current values when selecting a path.
    /// </summary>
    public static readonly StyledProperty<bool> ShowDataModelValuesProperty =
        AvaloniaProperty.Register<DataModelPicker, bool>(nameof(ShowDataModelValues));

    /// <summary>
    ///     A list of extra modules to show data models of.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<Module>?> ModulesProperty =
        AvaloniaProperty.Register<DataModelPicker, ObservableCollection<Module>?>(nameof(Modules), new ObservableCollection<Module>());

    /// <summary>
    ///     A list of data model view models to show
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<DataModelPropertiesViewModel>?> ExtraDataModelViewModelsProperty =
        AvaloniaProperty.Register<DataModelPicker, ObservableCollection<DataModelPropertiesViewModel>?>(nameof(ExtraDataModelViewModels), new ObservableCollection<DataModelPropertiesViewModel>());

    /// <summary>
    ///     A list of types to filter the selectable paths on.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<Type>?> FilterTypesProperty =
        AvaloniaProperty.Register<DataModelPicker, ObservableCollection<Type>?>(nameof(FilterTypes), new ObservableCollection<Type>());

    private bool _attached;
    private bool _flyoutActive;
    private Button? _button;
    private DataModelPickerFlyout? _flyout;

    static DataModelPickerButton()
    {
        ShowFullPathProperty.Changed.Subscribe(ShowFullPathChanged);
        DataModelPathProperty.Changed.Subscribe(DataModelPathChanged);
    }

    /// <summary>
    ///     Gets or sets the placeholder to show when nothing is selected.
    /// </summary>
    public string Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the data model picker should show the full path of the selected value.
    /// </summary>
    public bool ShowFullPath
    {
        get => GetValue(ShowFullPathProperty);
        set => SetValue(ShowFullPathProperty, value);
    }

    /// <summary>
    ///     Gets a boolean indicating whether the data model picker has a value.
    /// </summary>
    public bool HasValue
    {
        get => GetValue(HasValueProperty);
        private set => SetValue(HasValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the desired flyout placement.
    /// </summary>
    public FlyoutPlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    ///     Gets or sets data model path.
    /// </summary>
    public DataModelPath? DataModelPath
    {
        get => GetValue(DataModelPathProperty);
        set => SetValue(DataModelPathProperty, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the data model picker should show current values when selecting a path.
    /// </summary>
    public bool ShowDataModelValues
    {
        get => GetValue(ShowDataModelValuesProperty);
        set => SetValue(ShowDataModelValuesProperty, value);
    }

    /// <summary>
    ///     A list of extra modules to show data models of.
    /// </summary>
    public ObservableCollection<Module>? Modules
    {
        get => GetValue(ModulesProperty);
        set => SetValue(ModulesProperty, value);
    }

    /// <summary>
    ///     A list of data model view models to show.
    /// </summary>
    public ObservableCollection<DataModelPropertiesViewModel>? ExtraDataModelViewModels
    {
        get => GetValue(ExtraDataModelViewModelsProperty);
        set => SetValue(ExtraDataModelViewModelsProperty, value);
    }

    /// <summary>
    ///     A list of types to filter the selectable paths on.
    /// </summary>
    public ObservableCollection<Type>? FilterTypes
    {
        get => GetValue(FilterTypesProperty);
        set => SetValue(FilterTypesProperty, value);
    }

    /// <summary>
    ///     Raised when the flyout opens.
    /// </summary>
    public event TypedEventHandler<DataModelPickerButton, EventArgs>? FlyoutOpened;

    /// <summary>
    ///     Raised when the flyout closes.
    /// </summary>
    public event TypedEventHandler<DataModelPickerButton, EventArgs>? FlyoutClosed;

    private static void DataModelPathChanged(AvaloniaPropertyChangedEventArgs<DataModelPath?> e)
    {
        if (e.Sender is not DataModelPickerButton self || !self._attached)
            return;

        if (e.OldValue.Value != null)
        {
            e.OldValue.Value.PathInvalidated -= self.PathValidationChanged;
            e.OldValue.Value.PathValidated -= self.PathValidationChanged;
            e.OldValue.Value.Dispose();
        }

        if (e.NewValue.Value != null)
        {
            e.NewValue.Value.PathInvalidated += self.PathValidationChanged;
            e.NewValue.Value.PathValidated += self.PathValidationChanged;
        }

        self.UpdateValueDisplay();
    }

    private static void ShowFullPathChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is DataModelPickerButton self)
            self.UpdateValueDisplay();
    }

    private void PathValidationChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateValueDisplay, DispatcherPriority.DataBind);
    }

    private void UpdateValueDisplay()
    {
        HasValue = DataModelPath != null && DataModelPath.IsValid;

        if (_button == null)
            return;

        if (!HasValue)
        {
            ToolTip.SetTip(_button, null);
            _button.Content = Placeholder;
        }
        else
        {
            string? formattedPath = null;
            if (DataModelPath != null && DataModelPath.IsValid)
                formattedPath = string.Join(" › ", DataModelPath.Segments.Where(s => s.GetPropertyDescription() != null).Select(s => s.GetPropertyDescription()!.Name));

            ToolTip.SetTip(_button, formattedPath);
            _button.Content = ShowFullPath
                ? formattedPath
                : DataModelPath?.Segments.LastOrDefault()?.GetPropertyDescription()?.Name ?? DataModelPath?.Segments.LastOrDefault()?.Identifier;
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_flyout == null)
            return;

        // Logic here is taken from Fluent Avalonia's ColorPicker which also reuses the same control since it's large
        _flyout.DataModelPicker.DataModelPath = DataModelPath;
        _flyout.DataModelPicker.ExtraDataModelViewModels = ExtraDataModelViewModels;
        _flyout.DataModelPicker.FilterTypes = FilterTypes;
        _flyout.DataModelPicker.Modules = Modules;
        _flyout.DataModelPicker.ShowDataModelValues = ShowDataModelValues;

        _flyout.Placement = Placement;
        _flyout.ShowAt(_button != null ? _button : this);
        _flyoutActive = true;

        FlyoutOpened?.Invoke(this, EventArgs.Empty);
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_button != null)
            _button.Click -= OnButtonClick;
        base.OnApplyTemplate(e);
        _button = e.NameScope.Find<Button>("MainButton");
        if (_button != null)
            _button.Click += OnButtonClick;
        UpdateValueDisplay();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = true;
        if (DataModelPath != null)
        {
            DataModelPath.PathInvalidated += PathValidationChanged;
            DataModelPath.PathValidated += PathValidationChanged;
        }

        _flyout ??= new DataModelPickerFlyout();
        _flyout.Confirmed += FlyoutOnConfirmed;
        _flyout.Closed += OnFlyoutClosed;
    }

    private void FlyoutOnConfirmed(DataModelPickerFlyout sender, object args)
    {
        if (_flyoutActive)
        {
            FlyoutClosed?.Invoke(this, EventArgs.Empty);
            _flyoutActive = false;

            if (_flyout != null)
                DataModelPath = _flyout.DataModelPicker.DataModelPath;
        }
    }

    private void OnFlyoutClosed(object? sender, EventArgs e)
    {
        if (_flyoutActive)
        {
            FlyoutClosed?.Invoke(this, EventArgs.Empty);
            _flyoutActive = false;
        }
    }


    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataModelPath != null)
        {
            DataModelPath.PathInvalidated -= PathValidationChanged;
            DataModelPath.PathValidated -= PathValidationChanged;
        }

        if (_flyout != null)
            _flyout.Closed -= OnFlyoutClosed;
    }

    #endregion
}