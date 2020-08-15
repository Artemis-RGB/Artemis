using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Shared.Services.Interfaces;
using Humanizer;
using Ninject;
using Ninject.Parameters;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertyGroupViewModel : LayerPropertyBaseViewModel
    {
        public enum ViewModelType
        {
            General,
            Transform,
            LayerBrushRoot,
            LayerEffectRoot,
            None
        }

        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
        private ViewModelType _groupType;
        private TreePropertyGroupViewModel _treePropertyGroupViewModel;
        private TimelinePropertyGroupViewModel _timelinePropertyGroupViewModel;

        public LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, PropertyGroupDescriptionAttribute propertyGroupDescription,
            IProfileEditorService profileEditorService, ILayerPropertyVmFactory layerPropertyVmFactory)
        {
            _layerPropertyVmFactory = layerPropertyVmFactory;
            ProfileEditorService = profileEditorService;

            LayerPropertyGroup = layerPropertyGroup;
            PropertyGroupDescription = propertyGroupDescription;

            TreePropertyGroupViewModel = _layerPropertyVmFactory.TreePropertyGroupViewModel(this);
            TimelinePropertyGroupViewModel = _layerPropertyVmFactory.TimelinePropertyGroupViewModel(this);

            // Generate a fallback name if the description does not contain one
            if (PropertyGroupDescription.Name == null)
            {
                var propertyInfo = LayerPropertyGroup.Parent?.GetType().GetProperties().FirstOrDefault(p => ReferenceEquals(p.GetValue(LayerPropertyGroup.Parent), LayerPropertyGroup));
                if (propertyInfo != null)
                    PropertyGroupDescription.Name = propertyInfo.Name.Humanize();
                else
                    PropertyGroupDescription.Name = "Unknown group";
            }

            LayerPropertyGroup.VisibilityChanged += LayerPropertyGroupOnVisibilityChanged;
            PopulateChildren();
            DetermineType();
        }

        public override bool IsVisible => !LayerPropertyGroup.IsHidden; 
        public IProfileEditorService ProfileEditorService { get; }
        public LayerPropertyGroup LayerPropertyGroup { get; }
        public PropertyGroupDescriptionAttribute PropertyGroupDescription { get; }

        public override bool IsExpanded
        {
            get => LayerPropertyGroup.ProfileElement.IsPropertyGroupExpanded(LayerPropertyGroup);
            set
            {
                LayerPropertyGroup.ProfileElement.SetPropertyGroupExpanded(LayerPropertyGroup, value);
                NotifyOfPropertyChange(nameof(IsExpanded));
            }
        }

        public ViewModelType GroupType
        {
            get => _groupType;
            set => SetAndNotify(ref _groupType, value);
        }

        public TreePropertyGroupViewModel TreePropertyGroupViewModel
        {
            get => _treePropertyGroupViewModel;
            set => SetAndNotify(ref _treePropertyGroupViewModel, value);
        }

        public TimelinePropertyGroupViewModel TimelinePropertyGroupViewModel
        {
            get => _timelinePropertyGroupViewModel;
            set => SetAndNotify(ref _timelinePropertyGroupViewModel, value);
        }

        public override List<BaseLayerPropertyKeyframe> GetKeyframes(bool expandedOnly)
        {
            var result = new List<BaseLayerPropertyKeyframe>();
            if (expandedOnly && !IsExpanded || LayerPropertyGroup.IsHidden)
                return result;

            foreach (var layerPropertyBaseViewModel in Children)
                result.AddRange(layerPropertyBaseViewModel.GetKeyframes(expandedOnly));

            return result;
        }

        public override void Dispose()
        {
            foreach (var layerPropertyBaseViewModel in Children)
                layerPropertyBaseViewModel.Dispose();

            LayerPropertyGroup.VisibilityChanged -= LayerPropertyGroupOnVisibilityChanged;
            TimelinePropertyGroupViewModel.Dispose();
        }

        public List<LayerPropertyBaseViewModel> GetAllChildren()
        {
            var result = new List<LayerPropertyBaseViewModel>();
            foreach (var layerPropertyBaseViewModel in Children)
            {
                result.Add(layerPropertyBaseViewModel);
                if (layerPropertyBaseViewModel is LayerPropertyGroupViewModel layerPropertyGroupViewModel)
                    result.AddRange(layerPropertyGroupViewModel.GetAllChildren());
            }

            return result;
        }

        private void DetermineType()
        {
            if (LayerPropertyGroup is LayerGeneralProperties)
                GroupType = ViewModelType.General;
            else if (LayerPropertyGroup is LayerTransformProperties)
                GroupType = ViewModelType.Transform;
            else if (LayerPropertyGroup.Parent == null && LayerPropertyGroup.LayerBrush != null)
                GroupType = ViewModelType.LayerBrushRoot;
            else if (LayerPropertyGroup.Parent == null && LayerPropertyGroup.LayerEffect != null)
                GroupType = ViewModelType.LayerEffectRoot;
            else
                GroupType = ViewModelType.None;
        }

        private void PopulateChildren()
        {
            // Get all properties and property groups and create VMs for them
            foreach (var propertyInfo in LayerPropertyGroup.GetType().GetProperties())
            {
                var propertyAttribute = (PropertyDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                var groupAttribute = (PropertyGroupDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                var value = propertyInfo.GetValue(LayerPropertyGroup);

                // Create VMs for properties on the group
                if (propertyAttribute != null && value is BaseLayerProperty baseLayerProperty)
                {
                    var viewModel = CreateLayerPropertyViewModel(baseLayerProperty, propertyAttribute);
                    if (viewModel != null)
                        Children.Add(viewModel);
                }
                // Create VMs for child groups on this group, resulting in a nested structure
                else if (groupAttribute != null && value is LayerPropertyGroup layerPropertyGroup)
                    Children.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(layerPropertyGroup, groupAttribute));
            }
        }

        private LayerPropertyBaseViewModel CreateLayerPropertyViewModel(BaseLayerProperty baseLayerProperty, PropertyDescriptionAttribute propertyDescription)
        {
            // Go through the pain of instantiating a generic type VM now via reflection to make things a lot simpler down the line
            var genericType = baseLayerProperty.GetType().Name == typeof(LayerProperty<>).Name
                ? baseLayerProperty.GetType().GetGenericArguments()[0]
                : baseLayerProperty.GetType().BaseType.GetGenericArguments()[0];

            // Only create entries for types supported by a tree input VM
            if (!genericType.IsEnum && ProfileEditorService.RegisteredPropertyEditors.All(r => r.SupportedType != genericType))
                return null;
            var genericViewModel = typeof(LayerPropertyViewModel<>).MakeGenericType(genericType);
            var parameters = new IParameter[]
            {
                new ConstructorArgument("layerProperty", baseLayerProperty),
                new ConstructorArgument("propertyDescription", propertyDescription)
            };

            return (LayerPropertyBaseViewModel) ProfileEditorService.Kernel.Get(genericViewModel, parameters);
        }

        private void LayerPropertyGroupOnVisibilityChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(IsVisible));
        }

        public void UpdateOrder(int order)
        {
            LayerPropertyGroup.LayerEffect.Order = order;
            NotifyOfPropertyChange(nameof(IsExpanded));
        }
    }
}