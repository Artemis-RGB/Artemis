using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    /// <summary>
    ///     Represents a base class for a view model that visualizes a part of the data model
    /// </summary>
    public abstract class DataModelVisualizationViewModel : ReactiveObject, IDisposable
    {
        private const int MaxDepth = 4;
        private ObservableCollection<DataModelVisualizationViewModel> _children;
        private DataModel? _dataModel;
        private bool _isMatchingFilteredTypes;
        private bool _isVisualizationExpanded;
        private DataModelVisualizationViewModel? _parent;
        private DataModelPropertyAttribute? _propertyDescription;
        private bool _populatedStaticChildren;

        internal DataModelVisualizationViewModel(DataModel? dataModel, DataModelVisualizationViewModel? parent, DataModelPath? dataModelPath)
        {
            _dataModel = dataModel;
            _children = new ObservableCollection<DataModelVisualizationViewModel>();
            _parent = parent;
            DataModelPath = dataModelPath;
            IsMatchingFilteredTypes = true;

            if (parent == null)
                IsRootViewModel = true;
            else
                PropertyDescription = DataModelPath?.GetPropertyDescription() ?? DataModel?.DataModelDescription;
        }

        /// <summary>
        ///     Gets a boolean indicating whether this view model is at the root of the data model
        /// </summary>
        public bool IsRootViewModel { get; protected set; }

        /// <summary>
        ///     Gets the data model path to the property this view model is visualizing
        /// </summary>
        public DataModelPath? DataModelPath { get; }

        /// <summary>
        ///     Gets a string representation of the path backing this model
        /// </summary>
        public string? Path => DataModelPath?.Path;

        /// <summary>
        ///     Gets the property depth of the view model
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        ///     Gets the data model backing this view model
        /// </summary>
        public DataModel? DataModel
        {
            get => _dataModel;
            protected set => this.RaiseAndSetIfChanged(ref _dataModel, value);
        }

        /// <summary>
        ///     Gets the property description of the property this view model is visualizing
        /// </summary>
        public DataModelPropertyAttribute? PropertyDescription
        {
            get => _propertyDescription;
            protected set => this.RaiseAndSetIfChanged(ref _propertyDescription, value);
        }

        /// <summary>
        ///     Gets the parent of this view model
        /// </summary>
        public DataModelVisualizationViewModel? Parent
        {
            get => _parent;
            protected set => this.RaiseAndSetIfChanged(ref _parent, value);
        }

        /// <summary>
        ///     Gets or sets an observable collection containing the children of this view model
        /// </summary>
        public ObservableCollection<DataModelVisualizationViewModel> Children
        {
            get => _children;
            set => this.RaiseAndSetIfChanged(ref _children, value);
        }

        /// <summary>
        ///     Gets a boolean indicating whether the property being visualized matches the types last provided to
        ///     <see cref="ApplyTypeFilter" />
        /// </summary>
        public bool IsMatchingFilteredTypes
        {
            get => _isMatchingFilteredTypes;
            private set => this.RaiseAndSetIfChanged(ref _isMatchingFilteredTypes, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the visualization is expanded, exposing the <see cref="Children" />
        /// </summary>
        public bool IsVisualizationExpanded
        {
            get => _isVisualizationExpanded;
            set
            {
                if (!this.RaiseAndSetIfChanged(ref _isVisualizationExpanded, value)) return;
                RequestUpdate();
            }
        }

        /// <summary>
        ///     Gets a user-friendly representation of the <see cref="DataModelPath" />
        /// </summary>
        public virtual string? DisplayPath => DataModelPath != null
            ? string.Join(" › ", DataModelPath.Segments.Select(s => s.GetPropertyDescription()?.Name ?? s.Identifier))
            : null;

        /// <summary>
        ///     Updates the datamodel and if in an parent, any children
        /// </summary>
        /// <param name="dataModelUIService">The data model UI service used during update</param>
        /// <param name="configuration">The configuration to apply while updating</param>
        public abstract void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration? configuration);

        /// <summary>
        ///     Gets the current value of the property being visualized
        /// </summary>
        /// <returns>The current value of the property being visualized</returns>
        public virtual object? GetCurrentValue()
        {
            if (IsRootViewModel)
                return null;

            return DataModelPath?.GetValue();
        }

        /// <summary>
        ///     Determines whether the provided types match the type of the property being visualized and sets the result in
        ///     <see cref="IsMatchingFilteredTypes" />
        /// </summary>
        /// <param name="looseMatch">Whether the type may be a loose match, meaning it can be cast or converted</param>
        /// <param name="filteredTypes">The types to filter</param>
        public void ApplyTypeFilter(bool looseMatch, params Type[]? filteredTypes)
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
            Type? type = DataModelPath?.GetPropertyType();
            if (type == null)
            {
                IsMatchingFilteredTypes = false;
                return;
            }

            if (looseMatch)
                IsMatchingFilteredTypes = filteredTypes.Any(t => t.IsCastableFrom(type) ||
                                                                 t == typeof(Enum) && type.IsEnum ||
                                                                 t == typeof(IEnumerable<>) && type.IsGenericEnumerable() ||
                                                                 type.IsGenericType && t == type.GetGenericTypeDefinition());
            else
                IsMatchingFilteredTypes = filteredTypes.Any(t => t == type || t == typeof(Enum) && type.IsEnum);
        }

        internal virtual int GetChildDepth()
        {
            return 0;
        }

        internal void PopulateProperties(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration? dataModelUpdateConfiguration)
        {
            if (IsRootViewModel && DataModel == null)
                return;

            Type? modelType = IsRootViewModel ? DataModel?.GetType() : DataModelPath?.GetPropertyType();
            if (modelType == null)
                throw new ArtemisSharedUIException("Failed to populate data model visualization properties, couldn't get a property type");

            // Add missing static children only once, they're static after all
            if (!_populatedStaticChildren)
            {
                foreach (PropertyInfo propertyInfo in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(t => t.MetadataToken))
                {
                    string childPath = AppendToPath(propertyInfo.Name);
                    if (Children.Any(c => c.Path != null && c.Path.Equals(childPath)))
                        continue;
                    if (propertyInfo.GetCustomAttribute<DataModelIgnoreAttribute>() != null)
                        continue;
                    MethodInfo? getMethod = propertyInfo.GetGetMethod();
                    if (getMethod == null || getMethod.GetParameters().Any())
                        continue;

                    DataModelVisualizationViewModel? child = CreateChild(dataModelUIService, childPath, GetChildDepth());
                    if (child != null)
                        Children.Add(child);
                }

                _populatedStaticChildren = true;
            }

            // Remove static children that should be hidden
            if (DataModel != null)
            {
                ReadOnlyCollection<PropertyInfo> hiddenProperties = DataModel.GetHiddenProperties();
                foreach (PropertyInfo hiddenProperty in hiddenProperties)
                {
                    string childPath = AppendToPath(hiddenProperty.Name);
                    DataModelVisualizationViewModel? toRemove = Children.FirstOrDefault(c => c.Path != null && c.Path == childPath);
                    if (toRemove != null)
                        Children.Remove(toRemove);
                }
            }

            // Add missing dynamic children
            object? value = Parent == null || Parent.IsRootViewModel ? DataModel : DataModelPath?.GetValue();
            if (value is DataModel dataModel)
            {
                foreach (string key in dataModel.DynamicChildren.Keys.ToList())
                {
                    string childPath = AppendToPath(key);
                    if (Children.Any(c => c.Path != null && c.Path.Equals(childPath)))
                        continue;

                    DataModelVisualizationViewModel? child = CreateChild(dataModelUIService, childPath, GetChildDepth());
                    if (child != null)
                        Children.Add(child);
                }
            }

            // Remove dynamic children that have been removed from the data model
            List<DataModelVisualizationViewModel> toRemoveDynamic = Children.Where(c => c.DataModelPath != null && !c.DataModelPath.IsValid).ToList();
            foreach (DataModelVisualizationViewModel dataModelVisualizationViewModel in toRemoveDynamic) 
                Children.Remove(dataModelVisualizationViewModel);
        }

        private DataModelVisualizationViewModel? CreateChild(IDataModelUIService dataModelUIService, string path, int depth)
        {
            if (DataModel == null)
                throw new ArtemisSharedUIException("Cannot create a data model visualization child VM for a parent without a data model");
            if (depth > MaxDepth)
                return null;

            DataModelPath dataModelPath = new(DataModel, path);
            if (!dataModelPath.IsValid)
                return null;

            PropertyInfo? propertyInfo = dataModelPath.GetPropertyInfo();
            Type? propertyType = dataModelPath.GetPropertyType();

            // Skip properties decorated with DataModelIgnore
            if (propertyInfo != null && Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                return null;
            // Skip properties that are in the ignored properties list of the respective profile module/data model expansion
            if (DataModel.GetHiddenProperties().Any(p => p.Equals(propertyInfo)))
                return null;

            if (propertyType == null)
                return null;

            // If a display VM was found, prefer to use that in any case
            DataModelDisplayViewModel? typeViewModel = dataModelUIService.GetDataModelDisplayViewModel(propertyType, PropertyDescription);
            if (typeViewModel != null)
                return new DataModelPropertyViewModel(DataModel, this, dataModelPath) {DisplayViewModel = typeViewModel, Depth = depth};
            // For primitives, create a property view model, it may be null that is fine
            if (propertyType.IsPrimitive || propertyType.IsEnum || propertyType == typeof(string) || propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return new DataModelPropertyViewModel(DataModel, this, dataModelPath) {Depth = depth};
            if (propertyType.IsGenericEnumerable())
                return new DataModelListViewModel(DataModel, this, dataModelPath) {Depth = depth};
            if (propertyType == typeof(DataModelEvent) || propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataModelEvent<>))
                return new DataModelEventViewModel(DataModel, this, dataModelPath) {Depth = depth};
            // For other value types create a child view model
            if (propertyType.IsClass || propertyType.IsStruct())
                return new DataModelPropertiesViewModel(DataModel, this, dataModelPath) {Depth = depth};

            return null;
        }

        private string AppendToPath(string toAppend)
        {
            if (string.IsNullOrEmpty(Path))
                return toAppend;

            StringBuilder builder = new();
            builder.Append(Path);
            builder.Append(".");
            builder.Append(toAppend);
            return builder.ToString();
        }

        private void RequestUpdate()
        {
            Parent?.RequestUpdate();
            OnUpdateRequested();
        }

        #region Events

        /// <summary>
        ///     Occurs when an update to the property this view model visualizes is requested
        /// </summary>
        public event EventHandler? UpdateRequested;

        /// <summary>
        ///     Invokes the <see cref="UpdateRequested" /> event
        /// </summary>
        protected virtual void OnUpdateRequested()
        {
            UpdateRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataModelPath?.Dispose();
                foreach (DataModelVisualizationViewModel dataModelVisualizationViewModel in Children) 
                    dataModelVisualizationViewModel.Dispose(true);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}