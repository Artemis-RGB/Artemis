using System;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.LayerEffects.Placeholder
{
    /// <summary>
    ///     Represents a layer effect that could not be loaded due to a missing plugin
    /// </summary>
    internal class PlaceholderLayerEffect : LayerEffect<PlaceholderProperties>
    {
        internal PlaceholderLayerEffect(LayerEffectEntity originalEntity, Guid placeholderFor)
        {
            OriginalEntity = originalEntity;
            PlaceholderFor = placeholderFor;

            EntityId = OriginalEntity.Id;
            Order = OriginalEntity.Order;
            Name = OriginalEntity.Name;
            Enabled = OriginalEntity.Enabled;
            HasBeenRenamed = OriginalEntity.HasBeenRenamed;
        }

        internal LayerEffectEntity OriginalEntity { get; }
        public Guid PlaceholderFor { get; }

        /// <inheritdoc />
        public override void EnableLayerEffect()
        {
        }

        /// <inheritdoc />
        public override void DisableLayerEffect()
        {
        }

        /// <inheritdoc />
        public override void Update(double deltaTime)
        {
        }

        /// <inheritdoc />
        public override void PreProcess(SKCanvas canvas, SKPath renderBounds, SKPaint paint)
        {
        }

        /// <inheritdoc />
        public override void PostProcess(SKCanvas canvas, SKPath renderBounds, SKPaint paint)
        {
        }

        internal override string GetEffectTypeName()
        {
            return OriginalEntity.EffectType;
        }
    }

    /// <summary>
    /// This is in place so that the UI has something to show
    /// </summary>
    internal class PlaceholderProperties : LayerPropertyGroup
    {
        protected override void PopulateDefaults()
        {
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}