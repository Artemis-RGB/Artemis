using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Plugins.DataModelExpansions;
using Artemis.Core.Plugins.DataModelExpansions.Attributes;
using Artemis.UI.Shared.Exceptions;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;
using Humanizer;
using Stylet;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
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
        private PropertyInfo _propertyInfo;

        internal DataModelVisualizationViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, PropertyInfo propertyInfo)
        {
            DataModel = dataModel;
            PropertyInfo = propertyInfo;
            Parent = parent;
            Children = new BindableCollection<DataModelVisualizationViewModel>();
            IsMatchingFilteredTypes = true;

            if (dataModel == null && parent == null && propertyInfo == null)
                IsRootViewModel = true;
            else
                GetDescription();
        }

        public bool IsRootViewModel { get; }
        public int Depth { get; set; }

        public DataModel DataModel
        {
            get => _dataModel;
            set => SetAndNotify(ref _dataModel, value);
        }

        public PropertyInfo PropertyInfo
        {
            get => _propertyInfo;
            protected set => SetAndNotify(ref _propertyInfo, value);
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

        public virtual string PropertyPath
        {
            get
            {
                if (Parent == null)
                    return PropertyInfo?.Name;

                if (PropertyInfo == null)
                    return Parent.PropertyPath;

                var parentPath = Parent.PropertyPath;
                return parentPath != null ? $"{parentPath}.{PropertyInfo.Name}" : PropertyInfo.Name;
            }
        }

        public virtual string DisplayPropertyPath
        {
            get
            {
                if (Parent == null)
                    return PropertyDescription?.Name;

                if (PropertyDescription == null)
                    return Parent.DisplayPropertyPath;

                var parentPath = Parent.DisplayPropertyPath;
                return parentPath != null ? $"{parentPath} › {PropertyDescription.Name}" : PropertyDescription.Name;
            }
        }

        /// <summary>
        ///     Updates the datamodel and if in an parent, any children
        /// </summary>
        /// <param name="dataModelUIService"></param>
        public abstract void Update(IDataModelUIService dataModelUIService);

        public virtual object GetCurrentValue()
        {
            try
            {
                if (PropertyInfo.GetGetMethod() == null)
                    return null;

                return Parent == null ? null : PropertyInfo.GetValue(Parent.GetCurrentValue());
            }
            catch (Exception)
            {
                // ignored, who knows what kind of shit can go wrong here...
                return null;
            }
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
                    child.ApplyTypeFilter(looseMatch, filteredTypes);

                IsMatchingFilteredTypes = true;
                return;
            }

            // If null is passed, clear the type filter
            if (filteredTypes == null || filteredTypes.Length == 0)
            {
                IsMatchingFilteredTypes = true;
                return;
            }

            // If this VM has no property info, assume it does not match
            if (PropertyInfo == null)
            {
                IsMatchingFilteredTypes = false;
                return;
            }

            if (looseMatch)
                IsMatchingFilteredTypes = filteredTypes.Any(t => t.IsCastableFrom(PropertyInfo.PropertyType));
            else
                IsMatchingFilteredTypes = filteredTypes.Any(t => t == PropertyInfo.PropertyType);
        }

        public DataModelVisualizationViewModel GetChildForCondition(DisplayConditionPredicate predicate, DisplayConditionSide side)
        {
            if (side == DisplayConditionSide.Left)
            {
                if (predicate.LeftDataModel == null || predicate.LeftPropertyPath == null)
                    return null;

                return GetChildByPath(predicate.LeftDataModel.PluginInfo.Guid, predicate.LeftPropertyPath);
            }

            if (predicate.RightDataModel == null || predicate.RightPropertyPath == null)
                return null;
            return GetChildByPath(predicate.RightDataModel.PluginInfo.Guid, predicate.RightPropertyPath);
        }

        public DataModelVisualizationViewModel GetChildByPath(Guid dataModelGuid, string propertyPath)
        {
            if (propertyPath == null)
                return null;

            // Ensure children are populated by requesting an update
            if (!IsVisualizationExpanded)
            {
                IsVisualizationExpanded = true;
                RequestUpdate();
                IsVisualizationExpanded = false;
            }

            var path = propertyPath.Split(".");
            var currentPart = path.First();
            if (IsRootViewModel)
            {
                var child = Children.FirstOrDefault(c => c.DataModel != null &&
                                                         c.DataModel.PluginInfo.Guid == dataModelGuid);
                return child?.GetChildByPath(dataModelGuid, propertyPath);
            }
            else
            {
                var child = Children.FirstOrDefault(c => c.DataModel != null &&
                                                         c.DataModel.PluginInfo.Guid == dataModelGuid && c.PropertyInfo?.Name == currentPart);
                if (child == null)
                    return null;

                if (path.Length > 1)
                    return child.GetChildByPath(dataModelGuid, string.Join(".", path.Skip(1)));
                return child;
            }
        }

        protected DataModelVisualizationViewModel CreateChild(IDataModelUIService dataModelUIService, PropertyInfo propertyInfo, int depth)
        {
            if (depth > MaxDepth)
                return null;
            // Skip properties decorated with DataModelIgnore
            if (Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                return null;
            // Skip properties that are in the ignored properties list of the respective profile module/data model expansion
            if (DataModel.GetHiddenProperties().Any(p => p.Equals(propertyInfo)))
                return null;

            // If a display VM was found, prefer to use that in any case
            var typeViewModel = dataModelUIService.GetDataModelDisplayViewModel(propertyInfo.PropertyType);
            if (typeViewModel != null)
                return new DataModelPropertyViewModel(DataModel, this, propertyInfo) {DisplayViewModel = typeViewModel, Depth = depth};
            // For primitives, create a property view model, it may be null that is fine
            if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string))
                return new DataModelPropertyViewModel(DataModel, this, propertyInfo) {Depth = depth};
            if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
                return new DataModelListViewModel(DataModel, this, propertyInfo) {Depth = depth};
            // For other value types create a child view model
            if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsStruct())
                return new DataModelPropertiesViewModel(DataModel, this, propertyInfo) {Depth = depth};

            return null;
        }

        private void RequestUpdate()
        {
            Parent?.RequestUpdate();
            OnUpdateRequested();
        }

        private void GetDescription()
        {
            // If this is the first child of a root view model, use the data model description
            if (Parent.IsRootViewModel)
                PropertyDescription = DataModel?.DataModelDescription;
            // Rely on property info for the description
            else if (PropertyInfo != null)
            {
                PropertyDescription = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(PropertyInfo, typeof(DataModelPropertyAttribute)) ??
                                      new DataModelPropertyAttribute {Name = PropertyInfo.Name.Humanize(), ResetsDepth = false};
            }
            else
                throw new ArtemisSharedUIException("Failed to get property description because plugin info is null but the parent has a datamodel");

            // If a property description was provided but the name is null, use the humanized property name
            if (PropertyDescription != null && PropertyDescription.Name == null && PropertyInfo != null)
                PropertyDescription.Name = PropertyInfo.Name.Humanize();
        }

        #region Events

        public event EventHandler UpdateRequested;

        protected virtual void OnUpdateRequested()
        {
            UpdateRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    public enum DisplayConditionSide
    {
        Left,
        Right
    }
}