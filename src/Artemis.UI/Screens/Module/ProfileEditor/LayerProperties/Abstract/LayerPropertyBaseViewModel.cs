using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract
{
    public abstract class LayerPropertyBaseViewModel : PropertyChangedBase, IDisposable
    {
        protected LayerPropertyBaseViewModel()
        {
            Children = new List<LayerPropertyBaseViewModel>();
        }

        public virtual bool IsExpanded { get; set; }
        public abstract bool IsVisible { get; }

        public List<LayerPropertyBaseViewModel> Children { get; set; }

        public abstract List<BaseLayerPropertyKeyframe> GetKeyframes(bool expandedOnly);
        public abstract void Dispose();
    }
}