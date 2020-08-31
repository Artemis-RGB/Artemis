using System;
using System.Collections.Generic;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Abstract
{
    public abstract class LayerPropertyBaseViewModel : PropertyChangedBase, IDisposable
    {
        private BindableCollection<LayerPropertyBaseViewModel> _children;
        private bool _isExpanded;

        protected LayerPropertyBaseViewModel()
        {
            Children = new BindableCollection<LayerPropertyBaseViewModel>();
        }

        public abstract bool IsVisible { get; }

        public virtual bool IsExpanded
        {
            get => _isExpanded;
            set => SetAndNotify(ref _isExpanded, value);
        }

        public BindableCollection<LayerPropertyBaseViewModel> Children
        {
            get => _children;
            set => SetAndNotify(ref _children, value);
        }

        public abstract void Dispose();

        public abstract List<BaseLayerPropertyKeyframe> GetKeyframes(bool expandedOnly);
    }
}