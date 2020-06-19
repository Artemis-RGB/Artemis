using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public abstract class PropertiesProfileElement : ProfileElement
    {
        internal abstract PropertiesEntity PropertiesEntity { get; }

        private SKPath _path;

        /// <summary>
        ///     Gets the path containing all the LEDs this entity is applied to, any rendering outside the entity Path is
        ///     clipped.
        /// </summary>
        public SKPath Path
        {
            get => _path;
            protected set
            {
                _path = value;
                // I can't really be sure about the performance impact of calling Bounds often but
                // SkiaSharp calls SkiaApi.sk_path_get_bounds (Handle, &rect); which sounds expensive
                Bounds = value?.Bounds ?? SKRect.Empty;
            }
        }

        /// <summary>
        ///     The bounds of this entity
        /// </summary>
        public SKRect Bounds { get; private set; }

        #region Property group expansion

        protected List<string> _expandedPropertyGroups;
        
        public bool IsPropertyGroupExpanded(LayerPropertyGroup layerPropertyGroup)
        {
            return _expandedPropertyGroups.Contains(layerPropertyGroup.Path);
        }

        public void SetPropertyGroupExpanded(LayerPropertyGroup layerPropertyGroup, bool expanded)
        {
            if (!expanded && IsPropertyGroupExpanded(layerPropertyGroup))
                _expandedPropertyGroups.Remove(layerPropertyGroup.Path);
            else if (expanded && !IsPropertyGroupExpanded(layerPropertyGroup))
                _expandedPropertyGroups.Add(layerPropertyGroup.Path);
        }

        #endregion

    }
}