using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.DataModelVisualization;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.DefaultTypes.DataModel.Display;
using DryIoc;

namespace Artemis.UI.Shared.Services;

internal class DataModelUIService : IDataModelUIService
{
    private readonly IDataModelService _dataModelService;
    private readonly IContainer _container;
    private readonly List<DataModelVisualizationRegistration> _registeredDataModelDisplays;
    private readonly List<DataModelVisualizationRegistration> _registeredDataModelEditors;
    private readonly PluginSetting<bool> _showFullPaths;
    private readonly PluginSetting<bool> _showDataModelValues;

    public DataModelUIService(IDataModelService dataModelService, IContainer container, ISettingsService settingsService)
    {
        _dataModelService = dataModelService;
        _container = container;
        _registeredDataModelEditors = new List<DataModelVisualizationRegistration>();
        _registeredDataModelDisplays = new List<DataModelVisualizationRegistration>();
        
        RegisteredDataModelEditors = new ReadOnlyCollection<DataModelVisualizationRegistration>(_registeredDataModelEditors);
        RegisteredDataModelDisplays = new ReadOnlyCollection<DataModelVisualizationRegistration>(_registeredDataModelDisplays);
        ShowFullPaths = settingsService.GetSetting("ProfileEditor.ShowFullPaths", true);
        ShowDataModelValues = settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
    }

    private DataModelInputViewModel InstantiateDataModelInputViewModel(DataModelVisualizationRegistration registration, DataModelPropertyAttribute? description, object? initialValue)
    {
        // This assumes the type can be converted, that has been checked when the VM was created
        if (initialValue != null && initialValue.GetType() != registration.SupportedType)
            initialValue = Convert.ChangeType(initialValue, registration.SupportedType);
        
        DataModelInputViewModel viewModel = (DataModelInputViewModel) registration.Plugin.Resolve(registration.ViewModelType, description, initialValue);
        viewModel.CompatibleConversionTypes = registration.CompatibleConversionTypes;
        return viewModel;
    }

    public IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelEditors { get; }
    public IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelDisplays { get; }
    public PluginSetting<bool> ShowFullPaths { get; }
    public PluginSetting<bool> ShowDataModelValues { get; }
    
    public DataModelPropertiesViewModel GetMainDataModelVisualization()
    {
        DataModelPropertiesViewModel viewModel = new(null, null, null);
        foreach (DataModel dataModelExpansion in _dataModelService.GetDataModels().Where(d => d.IsExpansion || d.Module.IsActivated).OrderBy(d => d.DataModelDescription.Name))
            viewModel.Children.Add(new DataModelPropertiesViewModel(dataModelExpansion, viewModel, new DataModelPath(dataModelExpansion)));

        // Update to populate children
        viewModel.Update(this, null);
        viewModel.UpdateRequested += (sender, args) => viewModel.Update(this, null);
        return viewModel;
    }

    public void UpdateModules(DataModelPropertiesViewModel mainDataModelVisualization)
    {
        List<DataModelVisualizationViewModel> disabledChildren = mainDataModelVisualization.Children
            .Where(d => d.DataModel != null && !d.DataModel.Module.IsEnabled)
            .ToList();
        foreach (DataModelVisualizationViewModel child in disabledChildren)
            mainDataModelVisualization.Children.Remove(child);

        foreach (DataModel dataModelExpansion in _dataModelService.GetDataModels().OrderBy(d => d.DataModelDescription.Name))
        {
            if (mainDataModelVisualization.Children.All(c => c.DataModel != dataModelExpansion))
                mainDataModelVisualization.Children.Add(
                    new DataModelPropertiesViewModel(dataModelExpansion, mainDataModelVisualization, new DataModelPath(dataModelExpansion))
                );
        }

        mainDataModelVisualization.Update(this, null);
    }

    public DataModelPropertiesViewModel? GetPluginDataModelVisualization(List<Module> modules, bool includeMainDataModel)
    {
        DataModelPropertiesViewModel root;
        // This will  contain any modules that are always available
        if (includeMainDataModel)
        {
            root = GetMainDataModelVisualization();
        }
        else
        {
            root = new DataModelPropertiesViewModel(null, null, null);
            root.UpdateRequested += (sender, args) => root.Update(this, null);
        }

        foreach (Module module in modules)
        {
            DataModel? dataModel = _dataModelService.GetPluginDataModel(module);
            if (dataModel == null)
                continue;

            root.Children.Add(new DataModelPropertiesViewModel(dataModel, root, new DataModelPath(dataModel)));
        }

        if (!root.Children.Any())
            return null;

        // Update to populate children
        root.Update(this, null);
        return root;
    }

    public DataModelVisualizationRegistration RegisterDataModelInput<T>(Plugin plugin, IReadOnlyCollection<Type>? compatibleConversionTypes = null) where T : DataModelInputViewModel
    {
        compatibleConversionTypes ??= new List<Type>();
        Type viewModelType = typeof(T);
        lock (_registeredDataModelEditors)
        {
            Type supportedType = viewModelType.BaseType!.GetGenericArguments()[0];
            DataModelVisualizationRegistration? existing = _registeredDataModelEditors.FirstOrDefault(r => r.SupportedType == supportedType);
            if (existing != null)
            {
                if (existing.Plugin != plugin)
                    throw new ArtemisSharedUIException($"Cannot register data model input for type {supportedType.Name} because an editor was already" +
                                                       $" registered by {existing.Plugin}");
                return existing;
            }

            _container.Register(viewModelType, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            // Create the registration
            DataModelVisualizationRegistration registration = new(this, RegistrationType.Input, plugin, supportedType, viewModelType)
            {
                // Apply the compatible conversion types to the registration
                CompatibleConversionTypes = compatibleConversionTypes
            };

            _registeredDataModelEditors.Add(registration);
            return registration;
        }
    }

    public DataModelVisualizationRegistration RegisterDataModelDisplay<T>(Plugin plugin) where T : DataModelDisplayViewModel
    {
        Type viewModelType = typeof(T);
        lock (_registeredDataModelDisplays)
        {
            Type supportedType = viewModelType.BaseType!.GetGenericArguments()[0];
            DataModelVisualizationRegistration? existing = _registeredDataModelDisplays.FirstOrDefault(r => r.SupportedType == supportedType);
            if (existing != null)
            {
                if (existing.Plugin != plugin)
                    throw new ArtemisSharedUIException($"Cannot register data model display for type {supportedType.Name} because an editor was already" +
                                                       $" registered by {existing.Plugin}");
                return existing;
            }

            _container.Register(viewModelType);
            DataModelVisualizationRegistration registration = new(this, RegistrationType.Display, plugin, supportedType, viewModelType);
            _registeredDataModelDisplays.Add(registration);
            return registration;
        }
    }

    public void RemoveDataModelInput(DataModelVisualizationRegistration registration)
    {
        lock (_registeredDataModelEditors)
        {
            if (_registeredDataModelEditors.Contains(registration))
            {
                registration.Unsubscribe();
                _registeredDataModelEditors.Remove(registration);

                _container.Unregister(registration.ViewModelType);
                _container.ClearCache(registration.ViewModelType);
            }
        }
    }

    public void RemoveDataModelDisplay(DataModelVisualizationRegistration registration)
    {
        lock (_registeredDataModelDisplays)
        {
            if (_registeredDataModelDisplays.Contains(registration))
            {
                registration.Unsubscribe();
                _registeredDataModelDisplays.Remove(registration);

                _container.Unregister(registration.ViewModelType);
                _container.ClearCache(registration.ViewModelType);
            }
        }
    }

    public DataModelDisplayViewModel? GetDataModelDisplayViewModel(Type propertyType, DataModelPropertyAttribute? description, bool fallBackToDefault)
    {
        lock (_registeredDataModelDisplays)
        {
            DataModelDisplayViewModel? result;

            DataModelVisualizationRegistration? match = _registeredDataModelDisplays.FirstOrDefault(d => d.SupportedType == propertyType);
            if (match != null)
                result = (DataModelDisplayViewModel) match.Plugin.Resolve(match.ViewModelType);
            else if (!fallBackToDefault)
                result = null;
            else
                result = _container.Resolve<DefaultDataModelDisplayViewModel>();

            if (result != null)
                result.PropertyDescription = description;

            return result;
        }
    }

    public DataModelInputViewModel? GetDataModelInputViewModel(Type propertyType, DataModelPropertyAttribute? description, object? initialValue, Action<object?, bool> updateCallback)
    {
        lock (_registeredDataModelEditors)
        {
            // Prefer a VM that natively supports the type
            DataModelVisualizationRegistration? match = _registeredDataModelEditors.FirstOrDefault(d => d.SupportedType == propertyType);
            // Fall back on a VM that supports the type through conversion
            if (match == null)
                match = _registeredDataModelEditors.FirstOrDefault(d => d.CompatibleConversionTypes != null && d.CompatibleConversionTypes.Contains(propertyType));
            // Lastly try getting an enum VM if the provided type is an enum
            if (match == null && propertyType.IsEnum)
                match = _registeredDataModelEditors.FirstOrDefault(d => d.SupportedType == typeof(Enum));

            if (match != null)
            {
                DataModelInputViewModel viewModel = InstantiateDataModelInputViewModel(match, description, initialValue);
                viewModel.UpdateCallback = updateCallback;
                return viewModel;
            }

            return null;
        }
    }
}