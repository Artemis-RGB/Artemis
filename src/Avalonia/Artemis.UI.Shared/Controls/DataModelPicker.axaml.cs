using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Services.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Shared.Controls;

public class DataModelPicker : TemplatedControl
{
    private static IDataModelUIService? _dataModelUIService;

    /// <summary>
    ///     Gets or sets data model path.
    /// </summary>
    public static readonly StyledProperty<DataModelPath?> DataModelPathProperty =
        AvaloniaProperty.Register<DataModelPicker, DataModelPath?>(nameof(DataModelPath), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    ///     Gets or sets the placeholder to show when nothing is selected.
    /// </summary>
    public static readonly StyledProperty<string> PlaceholderProperty =
        AvaloniaProperty.Register<DataModelPicker, string>(nameof(Placeholder), "Click to select");

    /// <summary>
    ///     Gets or sets a boolean indicating whether the data model picker should show current values when selecting a path.
    /// </summary>
    public static readonly StyledProperty<bool> ShowDataModelValuesProperty =
        AvaloniaProperty.Register<DataModelPicker, bool>(nameof(ShowDataModelValues));

    /// <summary>
    ///     Gets or sets a boolean indicating whether the data model picker should show the full path of the selected value.
    /// </summary>
    public static readonly StyledProperty<bool> ShowFullPathProperty =
        AvaloniaProperty.Register<DataModelPicker, bool>(nameof(ShowFullPath));

    /// <summary>
    ///     Gets or sets the brush to use when drawing the button.
    /// </summary>
    public static readonly StyledProperty<Brush> ButtonBrushProperty =
        AvaloniaProperty.Register<DataModelPicker, Brush>(nameof(ButtonBrush));

    /// <summary>
    ///     A list of extra modules to show data models of.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<Module>?> ModulesProperty =
        AvaloniaProperty.Register<DataModelPicker, ObservableCollection<Module>?>(nameof(Modules), new ObservableCollection<Module>());

    /// <summary>
    ///     The data model view model to show, if not provided one will be retrieved by the control.
    /// </summary>
    public static readonly StyledProperty<DataModelPropertiesViewModel?> DataModelViewModelProperty =
        AvaloniaProperty.Register<DataModelPicker, DataModelPropertiesViewModel?>(nameof(DataModelViewModel));

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

    /// <summary>
    ///     Gets a boolean indicating whether the data model picker has a value.
    /// </summary>
    public static readonly StyledProperty<bool> HasValueProperty =
        AvaloniaProperty.Register<DataModelPicker, bool>(nameof(HasValue));

    private Button? _dataModelButton;
    private bool _attached;

    static DataModelPicker()
    {
        DataModelPathProperty.Changed.Subscribe(DataModelPathChanged);
        ShowFullPathProperty.Changed.Subscribe(ShowFullPathChanged);
        ModulesProperty.Changed.Subscribe(ModulesChanged);
        DataModelViewModelProperty.Changed.Subscribe(DataModelViewModelPropertyChanged);
        ExtraDataModelViewModelsProperty.Changed.Subscribe(ExtraDataModelViewModelsChanged);
    }

    private static void DataModelPathChanged(AvaloniaPropertyChangedEventArgs<DataModelPath?> e)
    {
        if (e.Sender is not DataModelPicker dataModelPicker)
            return;

        if (e.OldValue.Value != null)
        {
            e.OldValue.Value.PathInvalidated -= dataModelPicker.PathValidationChanged;
            e.OldValue.Value.PathValidated -= dataModelPicker.PathValidationChanged;
            e.OldValue.Value.Dispose();
        }

        if (!dataModelPicker._attached)
            return;

        dataModelPicker.UpdateValueDisplay();
        if (e.NewValue.Value != null)
        {
            e.NewValue.Value.PathInvalidated += dataModelPicker.PathValidationChanged;
            e.NewValue.Value.PathValidated += dataModelPicker.PathValidationChanged;
        }
    }

    private static void ShowFullPathChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is DataModelPicker dataModelPicker)
            dataModelPicker.UpdateValueDisplay();
    }

    private static void ModulesChanged(AvaloniaPropertyChangedEventArgs<ObservableCollection<Module>?> e)
    {
        if (e.Sender is DataModelPicker dataModelPicker)
            dataModelPicker.GetDataModel();
    }

    private static void DataModelViewModelPropertyChanged(AvaloniaPropertyChangedEventArgs<DataModelPropertiesViewModel?> e)
    {
        if (e.Sender is DataModelPicker && e.OldValue.Value != null)
            e.OldValue.Value.Dispose();
    }

    private static void ExtraDataModelViewModelsChanged(AvaloniaPropertyChangedEventArgs<ObservableCollection<DataModelPropertiesViewModel>?> e)
    {
        // TODO, the original did nothing here either and I can't remember why
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="DataModelPicker" /> class.
    /// </summary>
    public DataModelPicker()
    {
        SelectPropertyCommand = ReactiveCommand.Create<DataModelVisualizationViewModel>(selected => ExecuteSelectPropertyCommand(selected));
    }

    /// <summary>
    ///     Gets a command that selects the path by it's view model.
    /// </summary>
    public ReactiveCommand<DataModelVisualizationViewModel, Unit> SelectPropertyCommand { get; }

    /// <summary>
    ///     Internal, don't use.
    /// </summary>
    public static IDataModelUIService DataModelUIService
    {
        set
        {
            if (_dataModelUIService != null)
                throw new AccessViolationException("This is not for you to touch");
            _dataModelUIService = value;
        }
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
    ///     Gets or sets a boolean indicating whether the data model picker should show current values when selecting a path.
    /// </summary>
    public bool ShowDataModelValues
    {
        get => GetValue(ShowDataModelValuesProperty);
        set => SetValue(ShowDataModelValuesProperty, value);
    }

    /// <summary>
    ///     Gets or sets the brush to use when drawing the button.
    /// </summary>
    public Brush ButtonBrush
    {
        get => GetValue(ButtonBrushProperty);
        set => SetValue(ButtonBrushProperty, value);
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
    ///     The data model view model to show, if not provided one will be retrieved by the control.
    /// </summary>
    public DataModelPropertiesViewModel? DataModelViewModel
    {
        get => GetValue(DataModelViewModelProperty);
        set => SetValue(DataModelViewModelProperty, value);
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
    ///     Gets a boolean indicating whether the data model picker has a value.
    /// </summary>
    public bool HasValue
    {
        get => GetValue(HasValueProperty);
        private set => SetValue(HasValueProperty, value);
    }

    /// <summary>
    ///     Occurs when a new path has been selected
    /// </summary>
    public event EventHandler<DataModelSelectedEventArgs>? DataModelPathSelected;

    /// <summary>
    ///     Invokes the <see cref="DataModelPathSelected" /> event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnDataModelPathSelected(DataModelSelectedEventArgs e)
    {
        DataModelPathSelected?.Invoke(this, e);
    }

    private void ExecuteSelectPropertyCommand(DataModelVisualizationViewModel selected)
    {
        if (selected.DataModelPath == null)
            return;
        if (selected.DataModelPath.Equals(DataModelPath))
            return;

        DataModelPath = new DataModelPath(selected.DataModelPath);
        OnDataModelPathSelected(new DataModelSelectedEventArgs(DataModelPath));
    }

    private void GetDataModel()
    {
        if (_dataModelUIService == null)
            return;

        ChangeDataModel(_dataModelUIService.GetPluginDataModelVisualization(Modules?.ToList() ?? new List<Module>(), true));
    }

    private void ChangeDataModel(DataModelPropertiesViewModel? dataModel)
    {
        if (DataModelViewModel != null)
        {
            DataModelViewModel.Dispose();
            DataModelViewModel.UpdateRequested -= DataModelOnUpdateRequested;
        }

        DataModelViewModel = dataModel;
        if (DataModelViewModel != null)
            DataModelViewModel.UpdateRequested += DataModelOnUpdateRequested;
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _dataModelButton = e.NameScope.Find<Button>("DataModelButton");

        GetDataModel();
        UpdateValueDisplay();
    }

    #endregion

    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = true;
        if (DataModelPath != null)
        {
            DataModelPath.PathInvalidated += PathValidationChanged;
            DataModelPath.PathValidated += PathValidationChanged;
        }

        base.OnAttachedToVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (DataModelPath != null)
        {
            DataModelPath.PathInvalidated -= PathValidationChanged;
            DataModelPath.PathValidated -= PathValidationChanged;
        }

        DataModelViewModel?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    #endregion

    private void UpdateValueDisplay()
    {
        HasValue = DataModelPath != null && DataModelPath.IsValid;

        string? formattedPath = null;
        if (DataModelPath != null && DataModelPath.IsValid)
            formattedPath = string.Join(" › ", DataModelPath.Segments.Where(s => s.GetPropertyDescription() != null).Select(s => s.GetPropertyDescription()!.Name));

        if (_dataModelButton != null)
        {
            if (!HasValue)
            {
                ToolTip.SetTip(_dataModelButton, null);
                _dataModelButton.Content = Placeholder;
            }
            else
            {
                ToolTip.SetTip(_dataModelButton, formattedPath);
                _dataModelButton.Content = ShowFullPath
                    ? formattedPath
                    : DataModelPath?.Segments.LastOrDefault()?.GetPropertyDescription()?.Name ?? DataModelPath?.Segments.LastOrDefault()?.Identifier;
            }
        }
    }

    private void DataModelOnUpdateRequested(object? sender, EventArgs e)
    {
        DataModelViewModel?.ApplyTypeFilter(true, FilterTypes?.ToArray() ?? Type.EmptyTypes);
        if (ExtraDataModelViewModels == null) return;
        foreach (DataModelPropertiesViewModel extraDataModelViewModel in ExtraDataModelViewModels)
            extraDataModelViewModel.ApplyTypeFilter(true, FilterTypes?.ToArray() ?? Type.EmptyTypes);
    }

    private void PathValidationChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateValueDisplay, DispatcherPriority.DataBind);
    }
}