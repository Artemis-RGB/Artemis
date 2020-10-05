using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Shared
{
    public abstract class DataModelVisualizationViewModel : PropertyChangedBase
    {
        private const int MaxDepth = 4;
        private BindableCollection<DataModelVisualizationViewModel> _children;
        private DataModel _dataModel;
        private bool _isMatchingFilteredTypes;
        private bool _isVisualizationExpanded;
        private DataModelVisualizationViewModel _parent;
        private DataModelPropertyAttribute _propertyDescription;

        internal DataModelVisualizationViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath)
        {
            DataModel = dataModel;
            Parent = parent;
            DataModelPath = dataModelPath;
            Children = new BindableCollection<DataModelVisualizationViewModel>();
            IsMatchingFilteredTypes = true;

            if (dataModel == null && parent == null && dataModelPath == null)
                IsRootViewModel = true;
            else
                PropertyDescription = DataModelPath?.GetPropertyDescription() ?? DataModel.DataModelDescription;
        }

        public bool IsRootViewModel { get; }
        public DataModelPath DataModelPath { get; }
        public string Path => DataModelPath?.Path;

        public int Depth { get; set; }

        public DataModel DataModel
        {
            get => _dataModel;
            set => SetAndNotify(ref _dataModel, value);
        }

        public DataModelPropertyAttribute PropertyDescription
        {
            get => _propertyDescription;
            protected set => SetAndNotify(ref _propertyDescription, value);
        }

        public DataModelVisualizationViewModel Parent
        {
            get => _parent;
            protected set => SetAndNotify(ref _parent, value);
        }

        public BindableCollection<DataModelVisualizationViewModel> Children
        {
            get => _children;
            set => SetAndNotify(ref _children, value);
        }

        public bool IsMatchingFilteredTypes
        {
            get => _isMatchingFilteredTypes;
            set => SetAndNotify(ref _isMatchingFilteredTypes, value);
        }

        public bool IsVisualizationExpanded
        {
            get => _isVisualizationExpanded;
            set
            {
                if (!SetAndNotify(ref _isVisualizationExpanded, value)) return;
                RequestUpdate();
            }
        }

        public virtual string DisplayPath => Path.Replace(".", " › ");

        /// <summary>
        ///     Updates the datamodel and if in an parent, any children
        /// </summary>
        /// <param name="dataModelUIService"></param>
        public abstract void Update(IDataModelUIService dataModelUIService);

        public virtual object GetCurrentValue()
        {
            if (IsRootViewModel)
                return null;

            return DataModelPath.GetValue();
        }

        public void ApplyTypeFilter(bool looseMatch, params Type[] filteredTypes)
        {
            if (filteredTypes != null)
            {
                if (filteredTypes.All(t => t == null))
                    filteredTypes = null;
                else
                    filteredTypes = filteredTypes.Where(t => t != null).ToArray();
            }

            // If the VM has children, its own type is not relevant
            if (Children.Any())
            {
                foreach (var child in Children)
                    child?.ApplyTypeFilter(looseMatch, filteredTypes);

                IsMatchingFilteredTypes = true;
                return;
            }

            // If null is passed, clear the type filter
            if (filteredTypes == null || filteredTypes.Length == 0)
            {
                IsMatchingFilteredTypes = true;
                return;
            }

            // If the type couldn't be retrieved either way, assume false
            var type = DataModelPath.GetPropertyType();
            if (type == null)
            {
                IsMatchingFilteredTypes = false;
                return;
            }

            if (looseMatch)
                IsMatchingFilteredTypes = filteredTypes.Any(t => t.IsCastableFrom(type) || t == typeof(Enum) && type.IsEnum);
            else
                IsMatchingFilteredTypes = filteredTypes.Any(t => t == type || t == typeof(Enum) && type.IsEnum);
        }

        public DataModelVisualizationViewModel GetChildByPath(Guid dataModelGuid, string propertyPath)
        {
            if (DataModel.PluginInfo.Guid != dataModelGuid)
                return null;
            if (propertyPath == null)
                return null;
            if (Path.StartsWith(propertyPath))
                return null;

            // Ensure children are populated by requesting an update
            if (!IsVisualizationExpanded)
            {
                IsVisualizationExpanded = true;
                RequestUpdate();
                IsVisualizationExpanded = false;
            }

            foreach (var child in Children)
            {
                // Try the child itself first
                if (child.Path == propertyPath)
                    return child;

                // Try a child on the child next, this will go recursive
                var match = child.GetChildByPath(dataModelGuid, propertyPath);
                if (match != null)
                    return match;
            }

            return null;
        }

        internal virtual int GetChildDepth()
        {
            return 0;
        }

        internal void PopulateProperties(IDataModelUIService dataModelUIService, object overrideValue)
        {
            if (IsRootViewModel && overrideValue == null)
                return;

            Type modelType;
            if (overrideValue != null)
                modelType = overrideValue.GetType();
            else if (Parent.IsRootViewModel)
                modelType = DataModel.GetType();
            else
                modelType = DataModelPath.GetPropertyType();

            // Add missing static children
            foreach (var propertyInfo in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var childPath = Path != null ? $"{Path}.{propertyInfo.Name}" : propertyInfo.Name;
                if (Children.Any(c => c.Path != null && c.Path.Equals(childPath)))
                    continue;
                if (propertyInfo.GetCustomAttribute<DataModelIgnoreAttribute>() != null)
                    continue;

                var child = CreateChild(dataModelUIService, childPath, GetChildDepth(), overrideValue);
                if (child != null)
                    Children.Add(child);
            }

            // Remove static children that should be hidden
            ReadOnlyCollection<PropertyInfo> hiddenProperties;
            if (overrideValue != null && overrideValue is DataModel overrideValueDataModel)
                hiddenProperties = overrideValueDataModel.GetHiddenProperties();
            else
                hiddenProperties = DataModel.GetHiddenProperties();
            foreach (var hiddenProperty in hiddenProperties)
            {
                var childPath = Path != null ? $"{Path}.{hiddenProperty.Name}" : hiddenProperty.Name;
                var toRemove = Children.FirstOrDefault(c => c.Path != null && c.Path == childPath);
                if (toRemove != null)
                    Children.Remove(toRemove);
            }

            // Add missing dynamic children
            object value;
            if (overrideValue != null)
                value = overrideValue;
            else
                value = Parent.IsRootViewModel ? DataModel : DataModelPath.GetValue();
            if (value is DataModel dataModel)
            {
                foreach (var kvp in dataModel.DynamicDataModels)
                {
                    var childPath = Path != null ? $"{Path}.{kvp.Key}" : kvp.Key;
                    if (Children.Any(c => c.Path != null && c.Path.Equals(childPath)))
                        continue;

                    var child = CreateChild(dataModelUIService, childPath, GetChildDepth(), overrideValue);
                    if (child != null)
                        Children.Add(child);
                }
            }

            // Remove dynamic children that have been removed from the data model
            var toRemoveDynamic = Children.Where(c => !c.DataModelPath.IsValid).ToList();
            if (toRemoveDynamic.Any())
                Children.RemoveRange(toRemoveDynamic);
        }

        private DataModelVisualizationViewModel CreateChild(IDataModelUIService dataModelUIService, string path, int depth, object overrideValue)
        {
            if (depth > MaxDepth)
                return null;

            var dataModelPath = new DataModelPath(overrideValue ?? DataModel, path);
            if (!dataModelPath.IsValid)
                return null;

            var propertyInfo = dataModelPath.GetPropertyInfo();
            var propertyType = dataModelPath.GetPropertyType();

            // Skip properties decorated with DataModelIgnore
            if (propertyInfo != null && Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                return null;
            // Skip properties that are in the ignored properties list of the respective profile module/data model expansion
            if (DataModel.GetHiddenProperties().Any(p => p.Equals(propertyInfo)))
                return null;

            // If a display VM was found, prefer to use that in any case
            var typeViewModel = dataModelUIService.GetDataModelDisplayViewModel(propertyType);
            if (typeViewModel != null)
                return new DataModelPropertyViewModel(DataModel, this, dataModelPath) {DisplayViewModel = typeViewModel, Depth = depth};
            // For primitives, create a property view model, it may be null that is fine
            if (propertyType.IsPrimitive || propertyType.IsEnum || propertyType == typeof(string))
                return new DataModelPropertyViewModel(DataModel, this, dataModelPath) {Depth = depth};
            if (typeof(IList).IsAssignableFrom(propertyType))
                return new DataModelListViewModel(DataModel, this, dataModelPath) {Depth = depth};
            // For other value types create a child view model
            if (propertyType.IsClass || propertyType.IsStruct())
                return new DataModelPropertiesViewModel(DataModel, this, dataModelPath) {Depth = depth};

            return null;
        }

        private void RequestUpdate()
        {
            Parent?.RequestUpdate();
            OnUpdateRequested();
        }

        #region Events

        public event EventHandler UpdateRequested;

        protected virtual void OnUpdateRequested()
        {
            UpdateRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    public enum DataModelConditionSide
    {
        Left,
        Right
    }
}