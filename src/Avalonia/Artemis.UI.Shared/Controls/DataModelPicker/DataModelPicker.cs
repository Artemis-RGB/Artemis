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
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using ReactiveUI;

namespace Artemis.UI.Shared.Controls.DataModelPicker;

/// <summary>
///     Represents a data model picker picker that can be used to select a data model path.
/// </summary>
public class DataModelPicker : TemplatedControl
{
    /// <summary>
    ///     The data model UI service this picker should use.
    /// </summary>
    public static IDataModelUIService? DataModelUIService;

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

    static DataModelPicker()
    {
        ModulesProperty.Changed.Subscribe(ModulesChanged);
        DataModelViewModelProperty.Changed.Subscribe(DataModelViewModelPropertyChanged);
        ExtraDataModelViewModelsProperty.Changed.Subscribe(ExtraDataModelViewModelsChanged);
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

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        GetDataModel();
    }

    #endregion

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
        if (DataModelUIService == null)
            return;

        ChangeDataModel(DataModelUIService.GetPluginDataModelVisualization(Modules?.ToList() ?? new List<Module>(), true));
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

    private void DataModelOnUpdateRequested(object? sender, EventArgs e)
    {
        DataModelViewModel?.ApplyTypeFilter(true, FilterTypes?.ToArray() ?? Type.EmptyTypes);
        if (ExtraDataModelViewModels == null) return;
        foreach (DataModelPropertiesViewModel extraDataModelViewModel in ExtraDataModelViewModels)
            extraDataModelViewModel.ApplyTypeFilter(true, FilterTypes?.ToArray() ?? Type.EmptyTypes);
    }
}