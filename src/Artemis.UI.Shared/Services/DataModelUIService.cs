using System;
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
        private readonly IKernel _kernel;
        private readonly List<DataModelVisualizationRegistration> _registeredDataModelDisplays;
        private readonly List<DataModelVisualizationRegistration> _registeredDataModelEditors;

        public DataModelUIService(IDataModelService dataModelService, IKernel kernel)
        {
            _dataModelService = dataModelService;
            _kernel = kernel;
            _registeredDataModelEditors = new List<DataModelVisualizationRegistration>();
            _registeredDataModelDisplays = new List<DataModelVisualizationRegistration>();
        }

        public IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelEditors => _registeredDataModelEditors.AsReadOnly();
        public IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelDisplays => _registeredDataModelDisplays.AsReadOnly();

        public DataModelPropertiesViewModel GetMainDataModelVisualization()
        {
            var viewModel = new DataModelPropertiesViewModel(null, null, null);
            foreach (var dataModelExpansion in _dataModelService.GetDataModels())
                viewModel.Children.Add(new DataModelPropertiesViewModel(dataModelExpansion, viewModel, null));

            // Update to populate children
            viewModel.Update(this);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(this);
            return viewModel;
        }

        public DataModelPropertiesViewModel GetPluginDataModelVisualization(Plugin plugin, bool includeMainDataModel)
        {
            if (includeMainDataModel)
            {
                var mainDataModel = GetMainDataModelVisualization();

                // If the main data model already includes the plugin data model we're done
                if (mainDataModel.Children.Any(c => c.DataModel.PluginInfo.Instance == plugin))
                    return mainDataModel;
                // Otherwise get just the plugin data model and add it
                var pluginDataModel = GetPluginDataModelVisualization(plugin, false);
                if (pluginDataModel != null)
                    mainDataModel.Children.Add(pluginDataModel);

                return mainDataModel;
            }
            
            var dataModel = _dataModelService.GetPluginDataModel(plugin);
            if (dataModel == null)
                return null;

            var viewModel = new DataModelPropertiesViewModel(null, null, null);
            viewModel.Children.Add(new DataModelPropertiesViewModel(dataModel, viewModel, null));

            // Update to populate children
            viewModel.Update(this);
            viewModel.UpdateRequested += (sender, args) => viewModel.Update(this);
            return viewModel;
        }
        
        public DataModelVisualizationRegistration RegisterDataModelInput<T>(PluginInfo pluginInfo, IReadOnlyCollection<Type> compatibleConversionTypes = null) where T : DataModelInputViewModel
        {
            if (compatibleConversionTypes == null)
                compatibleConversionTypes = new List<Type>();
            var viewModelType = typeof(T);
            lock (_registeredDataModelEditors)
            {
                var supportedType = viewModelType.BaseType.GetGenericArguments()[0];
                var existing = _registeredDataModelEditors.FirstOrDefault(r => r.SupportedType == supportedType);
                if (existing != null)
                {
                    if (existing.PluginInfo != pluginInfo)
                    {
                        throw new ArtemisPluginException($"Cannot register data model input for type {supportedType.Name} " +
                                                         $"because an editor was already registered by {pluginInfo.Name}");
                    }

                    return existing;
                }

                _kernel.Bind(viewModelType).ToSelf();

                // Create the registration
                var registration = new DataModelVisualizationRegistration(this, RegistrationType.Input, pluginInfo, supportedType, viewModelType)
                {
                    // Apply the compatible conversion types to the registration
                    CompatibleConversionTypes = compatibleConversionTypes
                };

                _registeredDataModelEditors.Add(registration);
                return registration;
            }
        }

        public DataModelVisualizationRegistration RegisterDataModelDisplay<T>(PluginInfo pluginInfo) where T : DataModelDisplayViewModel
        {
            var viewModelType = typeof(T);
            lock (_registeredDataModelDisplays)
            {
                var supportedType = viewModelType.BaseType.GetGenericArguments()[0];
                var existing = _registeredDataModelDisplays.FirstOrDefault(r => r.SupportedType == supportedType);
                if (existing != null)
                {
                    if (existing.PluginInfo != pluginInfo)
                    {
                        throw new ArtemisPluginException($"Cannot register data model display for type {supportedType.Name} " +
                                                         $"because an editor was already registered by {pluginInfo.Name}");
                    }

                    return existing;
                }

                _kernel.Bind(viewModelType).ToSelf();
                var registration = new DataModelVisualizationRegistration(this, RegistrationType.Display, pluginInfo, supportedType, viewModelType);
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

        public DataModelDisplayViewModel GetDataModelDisplayViewModel(Type propertyType, bool fallBackToDefault)
        {
            lock (_registeredDataModelDisplays)
            {
                var match = _registeredDataModelDisplays.FirstOrDefault(d => d.SupportedType == propertyType);
                if (match != null)
                    return (DataModelDisplayViewModel) _kernel.Get(match.ViewModelType);
                return !fallBackToDefault ? null : _kernel.Get<DefaultDataModelDisplayViewModel>();
            }
        }

        public DataModelInputViewModel GetDataModelInputViewModel(Type propertyType, DataModelPropertyAttribute description, object initialValue, Action<object, bool> updateCallback)
        {
            lock (_registeredDataModelEditors)
            {
                // Prefer a VM that natively supports the type
                var match = _registeredDataModelEditors.FirstOrDefault(d => d.SupportedType == propertyType);
                // Fall back on a VM that supports the type through conversion
                if (match == null)
                    match = _registeredDataModelEditors.FirstOrDefault(d => d.CompatibleConversionTypes.Contains(propertyType));

                if (match != null)
                {
                    var viewModel = InstantiateDataModelInputViewModel(match, description, initialValue);
                    viewModel.UpdateCallback = updateCallback;
                    return viewModel;
                }

                return null;
            }
        }

        public DataModelDynamicViewModel GetDynamicSelectionViewModel(Module module)
        {
            return _kernel.Get<DataModelDynamicViewModel>(new ConstructorArgument("module", module));
        }

        public DataModelStaticViewModel GetStaticInputViewModel(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            return _kernel.Get<DataModelStaticViewModel>(new ConstructorArgument("targetType", targetType));
        }

        private DataModelInputViewModel InstantiateDataModelInputViewModel(DataModelVisualizationRegistration registration, DataModelPropertyAttribute description, object initialValue)
        {
            // The view models expecting value types shouldn't be given null, avoid that
            if (registration.SupportedType.IsValueType)
            {
                if (initialValue == null)
                    initialValue = Activator.CreateInstance(registration.SupportedType);
            }

            // This assumes the type can be converted, that has been checked when the VM was created
            if (initialValue != null && initialValue.GetType() != registration.SupportedType)
                initialValue = Convert.ChangeType(initialValue, registration.SupportedType);

            var parameters = new IParameter[]
            {
                new ConstructorArgument("description", description),
                new ConstructorArgument("initialValue", initialValue)
            };
            var viewModel = (DataModelInputViewModel) _kernel.Get(registration.ViewModelType, parameters);
            viewModel.CompatibleConversionTypes = registration.CompatibleConversionTypes;
            return viewModel;
        }
    }
}