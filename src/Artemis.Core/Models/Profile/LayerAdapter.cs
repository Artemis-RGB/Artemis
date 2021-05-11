using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.AdaptionHints;

namespace Artemis.Core
{
    public class LayerAdapter : IStorageModel
    {
        internal LayerAdapter(Layer layer)
        {
            Layer = layer;
        }

        /// <summary>
        ///     Gets the layer this adapter can adapt
        /// </summary>
        public Layer Layer { get; }

        public List<IAdaptionHint> AdaptionHints { get; set; }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            AdaptionHints.Clear();
            // Kind of meh.
            // This leaves the adapter responsible for finding the right hint for the right entity, but it's gotta be done somewhere..
            foreach (IAdaptionHintEntity hintEntity in Layer.LayerEntity.AdaptionHints)
            {
                switch (hintEntity)
                {
                    case DeviceAdaptionHintEntity entity:
                        AdaptionHints.Add(new DeviceAdaptionHint(entity));
                        break;
                    case CategoryAdaptionHintEntity entity:
                        AdaptionHints.Add(new CategoryAdaptionHint(entity));
                        break;
                    case KeyboardSectionAdaptionHintEntity entity:
                        AdaptionHints.Add(new KeyboardSectionAdaptionHint(entity));
                        break;
                }
            }
        }

        /// <inheritdoc />
        public void Save()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}