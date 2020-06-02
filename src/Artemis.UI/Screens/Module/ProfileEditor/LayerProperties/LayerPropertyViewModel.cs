using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.PropertyInput;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
using Humanizer;
using Ninject;
using Ninject.Parameters;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel<T> : LayerPropertyViewModel
    {
        public LayerPropertyViewModel(IProfileEditorService profileEditorService, LayerProperty<T> layerProperty) : base(profileEditorService, layerProperty)
        {
            LayerProperty = layerProperty;

            TreePropertyViewModel = CreateTreePropertyViewModel();
            TimelinePropertyViewModel = new TimelinePropertyViewModel<T>(this, profileEditorService);

            TreePropertyBaseViewModel = TreePropertyViewModel;
            TimelinePropertyBaseViewModel = TimelinePropertyViewModel;

            // Generate a fallback name if the description does not contain one
            if (LayerProperty.PropertyDescription.Name == null)
            {
                var propertyInfo = LayerProperty.Parent?.GetType().GetProperties().FirstOrDefault(p => ReferenceEquals(p.GetValue(LayerProperty.Parent), LayerProperty));
                if (propertyInfo != null)
                    LayerProperty.PropertyDescription.Name = propertyInfo.Name.Humanize();
                else
                    LayerProperty.PropertyDescription.Name = $"Unknown {typeof(T).Name} property";
            }

            LayerProperty.VisibilityChanged += LayerPropertyOnVisibilityChanged;
        }

        public override bool IsVisible => !LayerProperty.IsHidden;

        public LayerProperty<T> LayerProperty { get; }

        public TreePropertyViewModel<T> TreePropertyViewModel { get; set; }
        public TimelinePropertyViewModel<T> TimelinePropertyViewModel { get; set; }

        public override List<BaseLayerPropertyKeyframe> GetKeyframes(bool expandedOnly)
        {
            if (LayerProperty.KeyframesEnabled && !LayerProperty.IsHidden)
                return LayerProperty.BaseKeyframes.ToList();
            return new List<BaseLayerPropertyKeyframe>();
        }

        public override void Dispose()
        {
            TreePropertyViewModel.Dispose();
            TimelinePropertyViewModel.Dispose();

            LayerProperty.VisibilityChanged -= LayerPropertyOnVisibilityChanged;
        }

        public void SetCurrentValue(T value, bool saveChanges)
        {
            LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            if (saveChanges)
                ProfileEditorService.UpdateSelectedProfileElement();
            else
                ProfileEditorService.UpdateProfilePreview();
        }

        private TreePropertyViewModel<T> CreateTreePropertyViewModel()
        {
            // Make sure there is a supported property editor VM, unless the type is an enum, then we'll use the EnumPropertyInputViewModel
            Type vmType = null;
            if (typeof(T).IsEnum)
                vmType = typeof(EnumPropertyInputViewModel<>).MakeGenericType(typeof(T));
            else
            {
                var registration = ProfileEditorService.RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(T));
                if (registration != null)
                    vmType = registration.ViewModelType;
            }

            if (vmType == null)
                throw new ArtemisUIException($"Cannot create a tree property view model for type {typeof(T)}, found no matching property editor");

            var parameters = new IParameter[]
            {
                new ConstructorArgument("layerProperty", LayerProperty)
            };
            return new TreePropertyViewModel<T>(this, (PropertyInputViewModel<T>) ProfileEditorService.Kernel.Get(vmType, parameters), ProfileEditorService);
        }

        private void LayerPropertyOnVisibilityChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(IsVisible));
        }
    }

    public abstract class LayerPropertyViewModel : LayerPropertyBaseViewModel
    {
        protected LayerPropertyViewModel(IProfileEditorService profileEditorService, BaseLayerProperty baseLayerProperty)
        {
            ProfileEditorService = profileEditorService;
            BaseLayerProperty = baseLayerProperty;
        }

        public IProfileEditorService ProfileEditorService { get; }
        public BaseLayerProperty BaseLayerProperty { get; }

        public TreePropertyViewModel TreePropertyBaseViewModel { get; set; }
        public TimelinePropertyViewModel TimelinePropertyBaseViewModel { get; set; }
    }
}