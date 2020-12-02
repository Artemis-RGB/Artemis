using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.UI.Exceptions;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline.Models
{
    public class KeyframeClipboardModel
    {
        public Dictionary<string, KeyframeEntity> KeyframeEntities { get; set; }
        public KeyframeClipboardModel(List<ILayerPropertyKeyframe> keyframes)
        {
            KeyframeEntities = new Dictionary<string, KeyframeEntity>();
            foreach (ILayerPropertyKeyframe keyframe in keyframes)
            {
                KeyframeEntities.Add(keyframe.UntypedLayerProperty.Path, );
            }
        }

        public void Paste(RenderProfileElement target, TimeSpan pastePosition)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (HasBeenPasted)
                throw new ArtemisUIException("Clipboard model can only be pasted once");

            HasBeenPasted = true;
        }

        public bool HasBeenPasted { get; set; }
    }
}