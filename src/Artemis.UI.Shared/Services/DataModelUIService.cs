﻿using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.DefaultTypes.DataModel.Display;
using Artemis.UI.Shared.Input;
using Ninject;
using Ninject.Parameters;

namespace Artemis.UI.Shared.Services
{
    internal class DataModelUIService : IDataModelUIService
    {
        private readonly IDataModelService _dataModelService;
        private readonly IDataModelVmFactory _dataModelVmFactory;
        private readonly IKernel _kernel;
        private readonly List<DataModelVisualizationRegistration> _registeredDataModelDisplays;
        private readonly List<DataModelVisualizationRegistration> _registeredDataModelEditors;

        public DataModelUIService(IDataModelService dataModelService, IDataModelVmFactory dataModelVmFactory, IKernel kernel)
        {
            _dataModelService = dataModelService;
            _dataModelVmFactory = dataModelVmFactory;
            _kernel = kernel;
            _registeredDataModelEditors = new List<DataModelVisualizationRegistration>();
            _registeredDataModelDisplays = new List<DataModelVisualizationRegistration>();
        }

        public IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelEditors => _registeredDataModelEditors.AsReadOnly();
        public IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelDisplays => _registeredDataModelDisplays.AsReadOnly();

        public DataModelPropertiesViewModel GetMainDataModelVisualization()
        {
            DataModelPropertiesViewModel viewModel = new(null, null, null);
            foreach (DataModel dataModelExpansion in _dataModelService.GetDataModels().OrderBy(d => d.DataModelDescription.Name))
                viewModel.Children.Add(new DataModelPropertiesViewModel(dataModelExpansion, viewModel, new DataModelPath(dataModelExpansion)));

            // Update to populate children
            viewModel.Update(this, null);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(this, null);
            return viewModel;
        }

        public void UpdateModules(DataModelPropertiesViewModel mainDataModelVisualization)
        {
            List<DataModelVisualizationViewModel> disabledChildren = mainDataModelVisualization.Children
                .Where(d => d.DataModel != null && !d.DataModel.Feature.IsEnabled)
                .ToList();
            foreach (DataModelVisualizationViewModel child in disabledChildren)
                mainDataModelVisualization.Children.Remove(child);

            foreach (DataModel dataModelExpansion in _dataModelService.GetDataModels().OrderBy(d => d.DataModelDescription.Name))
            {
                if (mainDataModelVisualization.Children.All(c => c.DataModel != dataModelExpansion))
                {
                    mainDataModelVisualization.Children.Add(
                        new DataModelPropertiesViewModel(dataModelExpansion, mainDataModelVisualization, new DataModelPath(dataModelExpansion))
                    );
                }
            }

            mainDataModelVisualization.Update(this, null);
        }

        public DataModelPropertiesViewModel? GetPluginDataModelVisualization(PluginFeature pluginFeature, bool includeMainDataModel)
        {
            if (includeMainDataModel)
            {
                DataModelPropertiesViewModel mainDataModel = GetMainDataModelVisualization();

                // If the main data model already includes the plugin data model we're done
                if (mainDataModel.Children.Any(c => c.DataModel?.Feature == pluginFeature))
                    return mainDataModel;
                // Otherwise get just the plugin data model and add it
                DataModelPropertiesViewModel? pluginDataModel = GetPluginDataModelVisualization(pluginFeature, false);
                if (pluginDataModel != null)
                    mainDataModel.Children.Add(pluginDataModel);

                return mainDataModel;
            }

            DataModel? dataModel = _dataModelService.GetPluginDataModel(pluginFeature);
            if (dataModel == null)
                return null;

            DataModelPropertiesViewModel viewModel = new(null, null, null);
            viewModel.Children.Add(new DataModelPropertiesViewModel(dataModel, viewModel, new DataModelPath(dataModel)));

            // Update to populate children
            viewModel.Update(this, null);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(this, null);
            return viewModel;
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

                _kernel.Bind(viewModelType).ToSelf();

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

                _kernel.Bind(viewModelType).ToSelf();
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

                    _kernel.Unbind(registration.ViewModelType);
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

                    _kernel.Unbind(registration.ViewModelType);
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
                {
                    // If this ever happens something is likely wrong with the plugin unload detection
                    if (match.Plugin.Kernel == null)
                        throw new ArtemisSharedUIException("Cannot GetDataModelDisplayViewModel for a registration by an uninitialized plugin");

                    result = (DataModelDisplayViewModel) match.Plugin.Kernel.Get(match.ViewModelType);
                }
                else if (!fallBackToDefault)
                    result = null;
                else
                    result = _kernel.Get<DefaultDataModelDisplayViewModel>();

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

        public DataModelDynamicViewModel GetDynamicSelectionViewModel(Module module)
        {
            return _dataModelVmFactory.DataModelDynamicViewModel(module);
        }

        public DataModelStaticViewModel GetStaticInputViewModel(Type targetType, DataModelPropertyAttribute targetDescription)
        {
            return _dataModelVmFactory.DataModelStaticViewModel(targetType, targetDescription);
        }

        private DataModelInputViewModel InstantiateDataModelInputViewModel(DataModelVisualizationRegistration registration, DataModelPropertyAttribute? description, object? initialValue)
        {
            // This assumes the type can be converted, that has been checked when the VM was created
            if (initialValue != null && initialValue.GetType() != registration.SupportedType)
                initialValue = Convert.ChangeType(initialValue, registration.SupportedType);

            IParameter[] parameters =
            {
                new ConstructorArgument("targetDescription", description),
                new ConstructorArgument("initialValue", initialValue)
            };

            // If this ever happens something is likely wrong with the plugin unload detection
            if (registration.Plugin.Kernel == null)
                throw new ArtemisSharedUIException("Cannot InstantiateDataModelInputViewModel for a registration by an uninitialized plugin");

            DataModelInputViewModel viewModel = (DataModelInputViewModel) registration.Plugin.Kernel.Get(registration.ViewModelType, parameters);
            viewModel.CompatibleConversionTypes = registration.CompatibleConversionTypes;
            return viewModel;
        }
    }
}