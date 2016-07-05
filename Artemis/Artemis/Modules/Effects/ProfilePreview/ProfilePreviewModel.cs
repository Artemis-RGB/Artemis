using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Headset;
using Artemis.Profiles.Layers.Types.Mouse;

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
            return Profile.GetRenderLayers(DataModel, renderMice, renderHeadsets, true);
        }

        public override void Render(Bitmap keyboard, out Bitmap mouse, out Bitmap headset, bool renderMice,
            bool renderHeadsets)
        {
            mouse = null;
            headset = null;

            if (Profile == null || DataModel == null)
                return;

            // Get all enabled layers who's conditions are met
            var renderLayers = GetRenderLayers(renderMice, renderHeadsets);

            // Render the keyboard layer-by-layer
            var keyboardRect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(KeyboardScale);
            using (var g = Graphics.FromImage(keyboard))
            {
                // Fill the bitmap's background with black to avoid trailing colors on some keyboards
                g.Clear(Color.Black);
                Profile.DrawLayers(g, renderLayers.Where(rl => rl.MustDraw()), DataModel, keyboardRect, true, true);
            }

            // Render the mouse layer-by-layer
            var smallRect = new Rect(0, 0, 40, 40);
            mouse = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(mouse))
            {
                Profile.DrawLayers(g, renderLayers.Where(rl => rl.LayerType is MouseType), DataModel, smallRect,
                    true, true);
            }

            // Render the headset layer-by-layer
            headset = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(headset))
            {
                Profile.DrawLayers(g, renderLayers.Where(rl => rl.LayerType is HeadsetType), DataModel, smallRect,
                    true, true);
            }
        }
    }

    public class ProfilePreviewDataModel : IDataModel
    {
    }
}