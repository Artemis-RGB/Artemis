using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.LayerEffects
{
    /// <summary>
    ///     Represents a layer effect that could not be loaded due to a missing plugin
    /// </summary>
    public class PlaceholderLayerEffect : BaseLayerEffect
    {
        internal PlaceholderLayerEffect(LayerEffectEntity originalEntity)
        {
            OriginalEntity = originalEntity;
         
            EntityId = OriginalEntity.Id;
            Order = OriginalEntity.Order;
            Name = OriginalEntity.Name;
            Enabled = OriginalEntity.Enabled;
            HasBeenRenamed = OriginalEntity.HasBeenRenamed;
        }

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
        public override void PreProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
        }

        /// <inheritdoc />
        public override void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
        }

        internal override string GetEffectTypeName()
        {
            return OriginalEntity.EffectType;
        }

        internal override void Initialize()
        {
        }
    }
}