using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.UI.Exceptions;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline.Models
{
    public class KeyframesClipboardModel
    {
        // ReSharper disable once UnusedMember.Global - For JSON.NET
        public KeyframesClipboardModel()
        {
            ClipboardModels = new List<KeyframeClipboardModel>();
        }

        public KeyframesClipboardModel(IEnumerable<ILayerPropertyKeyframe> keyframes)
        {
            ClipboardModels = new List<KeyframeClipboardModel>();
            foreach (ILayerPropertyKeyframe keyframe in keyframes.OrderBy(k => k.Position))
                ClipboardModels.Add(new KeyframeClipboardModel(keyframe));
        }

        public List<KeyframeClipboardModel> ClipboardModels { get; set; }
        public bool HasBeenPasted { get; set; }

        public List<ILayerPropertyKeyframe> Paste(RenderProfileElement target, TimeSpan pastePosition)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (HasBeenPasted)
                throw new ArtemisUIException("Clipboard model can only be pasted once");

            List<ILayerPropertyKeyframe> results = new();
            if (!ClipboardModels.Any())
                return results;

            // Determine the offset by looking at the position of the first keyframe, start pasting from there
            TimeSpan offset = pastePosition - ClipboardModels.First().KeyframeEntity.Position;
            List<ILayerProperty> properties = target.GetAllLayerProperties();
            foreach (KeyframeClipboardModel clipboardModel in ClipboardModels)
            {
                ILayerPropertyKeyframe layerPropertyKeyframe = clipboardModel.Paste(properties, offset);
                if (layerPropertyKeyframe != null)
                    results.Add(layerPropertyKeyframe);
            }

            HasBeenPasted = true;
            return results;
        }
    }

    public class KeyframeClipboardModel
    {
        // ReSharper disable once UnusedMember.Global - For JSON.NET
        public KeyframeClipboardModel()
        {
        }

        public KeyframeClipboardModel(ILayerPropertyKeyframe layerPropertyKeyframe)
        {
            // FeatureId = layerPropertyKeyframe.UntypedLayerProperty.LayerPropertyGroup.Feature.Id;
            Path = layerPropertyKeyframe.UntypedLayerProperty.Path;
            KeyframeEntity = layerPropertyKeyframe.GetKeyframeEntity();
        }

        public string FeatureId { get; set; }
        public string Path { get; set; }
        public KeyframeEntity KeyframeEntity { get; set; }

        public ILayerPropertyKeyframe Paste(List<ILayerProperty> properties, TimeSpan offset)
        {
            // ILayerProperty property = properties.FirstOrDefault(p => p.LayerPropertyGroup.Feature.Id == FeatureId && p.Path == Path);
            // if (property != null)
            // {
            //     KeyframeEntity.Position += offset;
            //     ILayerPropertyKeyframe keyframe = property.AddKeyframeEntity(KeyframeEntity);
            //     KeyframeEntity.Position -= offset;
            //
            //     return keyframe;
            // }

            return null;
        }
    }
}