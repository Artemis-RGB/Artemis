using System;
using System.Collections;
using System.Collections.Generic;
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

        public bool IsRootViewModel { get; protected set; }
        public DataModelPath DataModelPath { get; }
        public string Path => DataModelPath?.Path;

        public int Depth { get; private set; }

        public DataModel DataModel
        {
            get => _dataModel;
            protected set => SetAndNotify(ref _dataModel, value);
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

        public virtual string DisplayPath => string.Join(" › ", DataModelPath.Segments.Select(s => s.GetPropertyDescription()?.Name ?? s.Identifier));

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
                foreach (DataModelVisualizationViewModel child in Children)
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
            Type type = DataModelPath?.GetPropertyType();
            if (type == null)
            {
                IsMatchingFilteredTypes = false;
                return;
            }

            if (looseMatch)
                IsMatchingFilteredTypes = filteredTypes.Any(t => t.IsCastableFrom(type) || 
                                                                 t == typeof(Enum) && type.IsEnum ||
                                                                 t == typeof(IEnumerable<>) && type.IsGenericEnumerable());
            else
                IsMatchingFilteredTypes = filteredTypes.Any(t => t == type || t == typeof(Enum) && type.IsEnum);
        }

        public DataModelVisualizationViewModel GetChildByPath(Guid dataModelGuid, string propertyPath)
        {
            if (!IsRootViewModel)
            {
                if (DataModel.PluginInfo.Guid != dataModelGuid)
                    return null;
                if (propertyPath == null)
                    return null;
                if (Path != null && Path.StartsWith(propertyPath, StringComparison.OrdinalIgnoreCase))
                    return null;
            }

            // Ensure children are populated by requesting an update
            if (!IsVisualizationExpanded)
            {
                IsVisualizationExpanded = true;
                RequestUpdate();
                IsVisualizationExpanded = false;
            }

            foreach (DataModelVisualizationViewModel child in Children)
            {
                // Try the child itself first
                if (child.Path == propertyPath)
                    return child;

                // Try a child on the child next, this will go recursive
                DataModelVisualizationViewModel match = child.GetChildByPath(dataModelGuid, propertyPath);
                if (match != null)
                    return match;
            }

            return null;
        }

        internal virtual int GetChildDepth()
        {
            return 0;
        }

        internal void PopulateProperties(IDataModelUIService dataModelUIService)
        {
            if (IsRootViewModel)
                return;

            Type modelType = Parent == null || Parent.IsRootViewModel ? DataModel.GetType() : DataModelPath.GetPropertyType();

            // Add missing static children
            foreach (PropertyInfo propertyInfo in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(t => t.MetadataToken))
            {
                string childPath = AppendToPath(propertyInfo.Name);
                if (Children.Any(c => c.Path != null && c.Path.Equals(childPath)))
                    continue;
                if (propertyInfo.GetCustomAttribute<DataModelIgnoreAttribute>() != null)
                    continue;
                MethodInfo getMethod = propertyInfo.GetGetMethod();
                if (getMethod == null || getMethod.GetParameters().Any())
                    continue;

                DataModelVisualizationViewModel child = CreateChild(dataModelUIService, childPath, GetChildDepth());
                if (child != null)
                    Children.Add(child);
            }

            // Remove static children that should be hidden
            ReadOnlyCollection<PropertyInfo> hiddenProperties = DataModel.GetHiddenProperties();
            foreach (PropertyInfo hiddenProperty in hiddenProperties)
            {
                string childPath = AppendToPath(hiddenProperty.Name);
                DataModelVisualizationViewModel toRemove = Children.FirstOrDefault(c => c.Path != null && c.Path == childPath);
                if (toRemove != null)
                    Children.Remove(toRemove);
            }

            // Add missing dynamic children
            object value = Parent == null || Parent.IsRootViewModel ? DataModel : DataModelPath.GetValue();
            if (value is DataModel dataModel)
            {
                foreach (KeyValuePair<string, DataModel> kvp in dataModel.DynamicDataModels)
                {
                    string childPath = AppendToPath(kvp.Key);
                    if (Children.Any(c => c.Path != null && c.Path.Equals(childPath)))
                        continue;

                    DataModelVisualizationViewModel child = CreateChild(dataModelUIService, childPath, GetChildDepth());
                    if (child != null)
                        Children.Add(child);
                }
            }

            // Remove dynamic children that have been removed from the data model
            List<DataModelVisualizationViewModel> toRemoveDynamic = Children.Where(c => !c.DataModelPath.IsValid).ToList();
            if (toRemoveDynamic.Any())
                Children.RemoveRange(toRemoveDynamic);
        }

        private DataModelVisualizationViewModel CreateChild(IDataModelUIService dataModelUIService, string path, int depth)
        {
            if (depth > MaxDepth)
                return null;

            DataModelPath dataModelPath = new DataModelPath(DataModel, path);
            if (!dataModelPath.IsValid)
                return null;

            PropertyInfo propertyInfo = dataModelPath.GetPropertyInfo();
            Type propertyType = dataModelPath.GetPropertyType();

            // Skip properties decorated with DataModelIgnore
            if (propertyInfo != null && Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                return null;
            // Skip properties that are in the ignored properties list of the respective profile module/data model expansion
            if (DataModel.GetHiddenProperties().Any(p => p.Equals(propertyInfo)))
                return null;

            // If a display VM was found, prefer to use that in any case
            DataModelDisplayViewModel typeViewModel = dataModelUIService.GetDataModelDisplayViewModel(propertyType, PropertyDescription);
            if (typeViewModel != null)
                return new DataModelPropertyViewModel(DataModel, this, dataModelPath) {DisplayViewModel = typeViewModel, Depth = depth};
            // For primitives, create a property view model, it may be null that is fine
            if (propertyType.IsPrimitive || propertyType.IsEnum || propertyType == typeof(string))
                return new DataModelPropertyViewModel(DataModel, this, dataModelPath) {Depth = depth};
            if (propertyType.IsGenericEnumerable())
                return new DataModelListViewModel(DataModel, this, dataModelPath) {Depth = depth};
            // For other value types create a child view model
            if (propertyType.IsClass || propertyType.IsStruct())
                return new DataModelPropertiesViewModel(DataModel, this, dataModelPath) {Depth = depth};
            
            return null;
        }

        private string AppendToPath(string toAppend)
        {
            return !string.IsNullOrEmpty(Path) ? $"{Path}.{toAppend}" : toAppend;
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
}