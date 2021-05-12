using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.AdaptionHints;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a hint that adapts layers to a certain category of devices
    /// </summary>
    public class CategoryAdaptionHint : IAdaptionHint
    {
        public CategoryAdaptionHint()
        {
        }

        internal CategoryAdaptionHint(CategoryAdaptionHintEntity entity)
        {
            Category = (DeviceCategory) entity.Category;
            Skip = entity.Skip;
            LimitAmount = entity.LimitAmount;
            Amount = entity.Amount;
        }

        /// <summary>
        ///     Gets or sets the category of devices LEDs will be applied to
        /// </summary>
        public DeviceCategory Category { get; set; }

        /// <summary>
        ///     Gets or sets the amount of devices to skip
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether a limited amount of devices should be used
        /// </summary>
        public bool LimitAmount { get; set; }

        /// <summary>
        ///     Gets or sets the amount of devices to limit to if <see cref="LimitAmount" /> is <see langword="true" />
        /// </summary>
        public int Amount { get; set; }

        #region Implementation of IAdaptionHint

        /// <inheritdoc />
        public void Apply(Layer layer, List<ArtemisDevice> devices)
        {
            IEnumerable<ArtemisDevice> matches = devices
                .Where(d => d.Categories.Contains(Category))
                .OrderBy(d => d.Rectangle)
                .Skip(Skip);
            if (LimitAmount)
                matches = matches.Take(Amount);

            foreach (ArtemisDevice artemisDevice in matches)
                layer.AddLeds(artemisDevice.Leds);
        }

        /// <inheritdoc />
        public IAdaptionHintEntity GetEntry()
        {
            return new CategoryAdaptionHintEntity {Amount = Amount, LimitAmount = LimitAmount, Category = (int) Category, Skip = Skip};
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Category adaption - {nameof(Category)}: {Category}, {nameof(Skip)}: {Skip}, {nameof(LimitAmount)}: {LimitAmount}, {nameof(Amount)}: {Amount}";
        }
    }
}