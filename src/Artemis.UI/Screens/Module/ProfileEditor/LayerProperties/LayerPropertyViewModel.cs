using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Utilities;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel<T> : LayerPropertyViewModel
    {
        private readonly IKernel _kernel;
        private readonly IProfileEditorService _profileEditorService;
        private bool _keyframesEnabled;
        private bool _isExpanded;

        public LayerPropertyViewModel(LayerProperty<T> layerProperty, LayerPropertyViewModel parent, IKernel kernel, IProfileEditorService profileEditorService)
        {
            _kernel = kernel;
            _profileEditorService = profileEditorService;
            _keyframesEnabled = layerProperty.KeyframesEnabled;

            LayerProperty = layerProperty;
            Parent = parent;
            Children = new List<LayerPropertyViewModel>();

            // TODO: Get from attribute
            IsExpanded = false;

            Parent?.Children.Add(this);
        }

        public LayerProperty<T> LayerProperty { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnExpandedStateChanged();
            }
        }

        public bool KeyframesEnabled
        {
            get => _keyframesEnabled;
            set
            {
                _keyframesEnabled = value;
                UpdateKeyframes();
            }
        }

        public PropertyInputViewModel GetPropertyInputViewModel()
        {
            // If the type is an enum type, search for Enum instead.
            var type = typeof(T);
            if (type.IsEnum)
                type = typeof(Enum);

            var match = _kernel.Get<List<PropertyInputViewModel>>().FirstOrDefault(p => p.CompatibleTypes.Contains(type));
            if (match == null)
                return null;

            match.Initialize(this);
            return match;
        }

        private void UpdateKeyframes()
        {
            
            // Either create a new first keyframe or clear all the keyframes
            if (_keyframesEnabled)
                LayerProperty.AddKeyframe(new LayerPropertyKeyframe<T>(LayerProperty.CurrentValue, _profileEditorService.CurrentTime, Easings.Functions.Linear));
            else
                LayerProperty.ClearKeyframes();

            // Force the keyframe engine to update, the new keyframe is the current keyframe
            LayerProperty.KeyframesEnabled = _keyframesEnabled;
            LayerProperty.Update(0);

            _profileEditorService.UpdateSelectedProfileElement();
        }

        #region Events

        public event EventHandler<EventArgs> ExpandedStateChanged;

        protected virtual void OnExpandedStateChanged()
        {
            ExpandedStateChanged?.Invoke(this, EventArgs.Empty);
            foreach (var layerPropertyViewModel in Children)
                layerPropertyViewModel.OnExpandedStateChanged();
        }

        #endregion
    }

    public class LayerPropertyViewModel : PropertyChangedBase
    {
        public LayerPropertyViewModel Parent { get; protected set; }
        public List<LayerPropertyViewModel> Children { get; protected set; }
    }
}