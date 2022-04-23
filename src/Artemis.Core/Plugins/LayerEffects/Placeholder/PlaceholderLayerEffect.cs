using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.LayerEffects.Placeholder
{
    /// <summary>
    ///     Represents a layer effect that could not be loaded due to a missing plugin
    /// </summary>
    internal class PlaceholderLayerEffect : LayerEffect<PlaceholderProperties>
    {
        internal PlaceholderLayerEffect(LayerEffectEntity originalEntity, string placeholderFor)
        {
            OriginalEntity = originalEntity;
            PlaceholderFor = placeholderFor;

            LayerEffectEntity = originalEntity;
            Order = OriginalEntity.Order;
            Name = OriginalEntity.Name;
            HasBeenRenamed = OriginalEntity.HasBeenRenamed;
        }

        public string PlaceholderFor { get; }

        internal LayerEffectEntity OriginalEntity { get; }

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
        public override void PreProcess(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
        }

        /// <inheritdoc />
        public override void PostProcess(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
        }

        internal override string GetEffectTypeName()
        {
            return OriginalEntity.EffectType;
        }
    }

    /// <summary>
    ///     This is in place so that the UI has something to show
    /// </summary>
    internal class PlaceholderProperties : LayerEffectPropertyGroup
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