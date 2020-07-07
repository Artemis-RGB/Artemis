using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Shared.DataModelVisualization;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Shared.Services
{
    public class DataModelVisualizationService : IDataModelVisualizationService
    {
        private readonly IDataModelService _dataModelService;
        private readonly IKernel _kernel;
        private readonly List<DataModelVisualizationRegistration> _registeredDataModelDisplays;
        private readonly List<DataModelVisualizationRegistration> _registeredDataModelEditors;

        public DataModelVisualizationService(IDataModelService dataModelService, IKernel kernel)
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
            foreach (var dataModelExpansion in _dataModelService.DataModelExpansions)
                viewModel.Children.Add(new DataModelPropertiesViewModel(dataModelExpansion, viewModel, null));

            // Update to populate children
            viewModel.Update(this);
            return viewModel;
        }

        public DataModelPropertiesViewModel GetPluginDataModelVisualization(Plugin plugin)
        {
            var dataModel = _dataModelService.GetPluginDataModel(plugin);
            if (dataModel == null)
                return null;

            var viewModel = new DataModelPropertiesViewModel(null, null, null);
            viewModel.Children.Add(new DataModelPropertiesViewModel(dataModel, viewModel, null));

            // Update to populate children
            viewModel.Update(this);
            return viewModel;
        }

        public bool GetPluginExtendsDataModel(Plugin plugin)
        {
            return _dataModelService.GetPluginExtendsDataModel(plugin);
        }

        public DataModelVisualizationRegistration RegisterDataModelInput<T>(PluginInfo pluginInfo) where T : DataModelInputViewModel
        {
            var viewModelType = typeof(T);
            lock (_registeredDataModelEditors)
            {
                var supportedType = viewModelType.BaseType.GetGenericArguments()[0];
                var existing = _registeredDataModelEditors.FirstOrDefault(r => r.SupportedType == supportedType);
                if (existing != null)
                {
                    if (existing.PluginInfo != pluginInfo)
                        throw new ArtemisPluginException($"Cannot register data model input for type {supportedType.Name} " +
                                                         $"because an editor was already registered by {pluginInfo.Name}");
                    return existing;
                }

                _kernel.Bind(viewModelType).ToSelf();
                var registration = new DataModelVisualizationRegistration(this, RegistrationType.Input, pluginInfo, supportedType, viewModelType);
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
                        throw new ArtemisPluginException($"Cannot register data model display for type {supportedType.Name} " +
                                                         $"because an editor was already registered by {pluginInfo.Name}");
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

        public DataModelDisplayViewModel GetDataModelDisplayViewModel(Type propertyType)
        {
            lock (_registeredDataModelDisplays)
            {
                var match = _registeredDataModelDisplays.FirstOrDefault(d => d.SupportedType == propertyType);
                if (match != null)
                    return (DataModelDisplayViewModel) _kernel.Get(match.ViewModelType);
                return null;
            }
        }

        public DataModelInputViewModel GetDataModelInputViewModel(Type propertyType, DataModelPropertyAttribute description, object initialValue, Action<object, bool> updateCallback)
        {
            lock (_registeredDataModelEditors)
            {
                var match = _registeredDataModelEditors.FirstOrDefault(d => d.SupportedType == propertyType);
                if (match != null)
                {
                    // The view models expecting value types shouldn't be given null, avoid that
                    if (initialValue == null && propertyType.IsValueType)
                        initialValue = Activator.CreateInstance(propertyType);

                    var parameters = new IParameter[]
                    {
                        new ConstructorArgument("description", description),
                        new ConstructorArgument("initialValue", initialValue)
                    };
                    var viewModel = (DataModelInputViewModel) _kernel.Get(match.ViewModelType, parameters);
                    viewModel.UpdateCallback = updateCallback;
                    return viewModel;
                }

                return null;
            }
        }
    }

    public interface IDataModelVisualizationService : IArtemisSharedUIService
    {
        DataModelPropertiesViewModel GetMainDataModelVisualization();
        DataModelPropertiesViewModel GetPluginDataModelVisualization(Plugin plugin);

        /// <summary>
        ///     Determines whether the given plugin expands the main data model
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        bool GetPluginExtendsDataModel(Plugin plugin);

        DataModelVisualizationRegistration RegisterDataModelInput<T>(PluginInfo pluginInfo) where T : DataModelInputViewModel;
        DataModelVisualizationRegistration RegisterDataModelDisplay<T>(PluginInfo pluginInfo) where T : DataModelDisplayViewModel;
        void RemoveDataModelInput(DataModelVisualizationRegistration registration);
        void RemoveDataModelDisplay(DataModelVisualizationRegistration registration);

        DataModelDisplayViewModel GetDataModelDisplayViewModel(Type propertyType);
        DataModelInputViewModel GetDataModelInputViewModel(Type propertyType, DataModelPropertyAttribute description, object initialValue, Action<object, bool> updateCallback);
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelEditors { get; }
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelDisplays { get; }
    }
}