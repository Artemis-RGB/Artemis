using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Modules.Effects.ProfilePreview
{
    public class ProfilePreviewModel : EffectModel
    {
        public ProfilePreviewModel(MainManager mainManager) : base(mainManager, new ProfilePreviewDataModel())
        {
            Name = "Profile Preview";
        }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return Profile.GetRenderLayers<ProfilePreviewDataModel>(DataModel, renderMice, renderHeadsets, true);
        }

        public override void Render(out Bitmap keyboard, out Brush mouse, out Brush headset, bool renderMice, bool renderHeadsets)
        {
            keyboard = null;
            mouse = null;
            headset = null;

            if (Profile == null || DataModel == null)
                return;

            // Get all enabled layers who's conditions are met
            var renderLayers = GetRenderLayers(renderMice, renderHeadsets);

            // Render the keyboard layer-by-layer
            keyboard = Profile?.GenerateBitmap(renderLayers, DataModel, MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(4), true, true);
            // Render the first enabled mouse (will default to null if renderMice was false)
            mouse = Profile?.GenerateBrush(renderLayers.LastOrDefault(l => l.LayerType == LayerType.Mouse), DataModel);
            // Render the first enabled headset (will default to null if renderHeadsets was false)
            headset = Profile?.GenerateBrush(renderLayers.LastOrDefault(l => l.LayerType == LayerType.Headset), DataModel);
        }
    }

    public class ProfilePreviewDataModel : IDataModel
    {
    }
}