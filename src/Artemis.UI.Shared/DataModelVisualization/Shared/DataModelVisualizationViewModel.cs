using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.UI.Shared.Exceptions;
using Artemis.UI.Shared.Services;
using Humanizer;
using Stylet;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public abstract class DataModelVisualizationViewModel : PropertyChangedBase
    {
        private BindableCollection<DataModelVisualizationViewModel> _children;
        private DataModel _dataModel;
        private DataModelVisualizationViewModel _parent;
        private DataModelPropertyAttribute _propertyDescription;
        private PropertyInfo _propertyInfo;
        private bool _isMatchingFilteredTypes;

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

        public abstract void Update(IDataModelVisualizationService dataModelVisualizationService);

        public virtual object GetCurrentValue()
        {
            return Parent == null ? null : PropertyInfo.GetValue(Parent.GetCurrentValue());
        }

        public void ApplyTypeFilter(bool looseMatch, params Type[] filteredTypes)
        {
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

        public DataModelVisualizationViewModel GetChildByPath(Guid dataModelGuid, string propertyPath)
        {
            var path = propertyPath.Split(".");
            var currentPart = path.First();

            if (IsRootViewModel)
            {
                var child = Children.FirstOrDefault(c => c.DataModel.PluginInfo.Guid == dataModelGuid);
                return child?.GetChildByPath(dataModelGuid, propertyPath);
            }
            else
            {
                var child = Children.FirstOrDefault(c => c.DataModel.PluginInfo.Guid == dataModelGuid && c.PropertyInfo?.Name == currentPart);
                if (child == null)
                    return null;

                if (path.Length > 1)
                    return child.GetChildByPath(dataModelGuid, string.Join(".", path.Skip(1)));
                return child;
            }
        }

        public string GetCurrentPath()
        {
            if (Parent == null)
                return PropertyInfo?.Name;

            if (PropertyInfo == null)
                return Parent.GetCurrentPath();

            var parentPath = Parent.GetCurrentPath();
            return parentPath != null ? $"{parentPath}.{PropertyInfo.Name}" : PropertyInfo.Name;
        }

        protected DataModelVisualizationViewModel CreateChild(IDataModelVisualizationService dataModelVisualizationService, PropertyInfo propertyInfo)
        {
            // Skip properties decorated with DataModelIgnore
            if (Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                return null;

            // If a display VM was found, prefer to use that in any case
            var typeViewModel = dataModelVisualizationService.GetDataModelDisplayViewModel(propertyInfo.PropertyType);
            if (typeViewModel != null)
                return new DataModelPropertyViewModel(DataModel, this, propertyInfo) {DisplayViewModel = typeViewModel};
            // For primitives, create a property view model, it may be null that is fine
            if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string))
                return new DataModelPropertyViewModel(DataModel, this, propertyInfo);
            if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
                return new DataModelListViewModel(DataModel, this, propertyInfo);
            // For other value types create a child view model
            if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsStruct())
                return new DataModelPropertiesViewModel(DataModel, this, propertyInfo);

            return null;
        }

        private void GetDescription()
        {
            // If this is the first child of a root view model, use the data model description
            if (Parent.IsRootViewModel)
                PropertyDescription = DataModel?.DataModelDescription;
            // Rely on property info for the description
            else if (PropertyInfo != null)
                PropertyDescription = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(PropertyInfo, typeof(DataModelPropertyAttribute)) ??
                                      new DataModelPropertyAttribute {Name = PropertyInfo.Name.Humanize()};
            else
                throw new ArtemisSharedUIException("Failed to get property description because plugin info is null but the parent has a datamodel");

            // If a property description was provided but the name is null, use the humanized property name
            if (PropertyDescription != null && PropertyDescription.Name == null && PropertyInfo != null)
                PropertyDescription.Name = PropertyInfo.Name.Humanize();
        }
    }
}