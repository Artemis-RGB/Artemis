﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

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

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly, true);
        }

        public override void Render(RenderFrame frame, bool keyboardOnly)
        {
            if (Profile == null || DataModel == null || MainManager.DeviceManager.ActiveKeyboard == null)
                return;

            lock (DataModel)
            {
                // Get all enabled layers who's conditions are met
                var renderLayers = GetRenderLayers(keyboardOnly);

                // Render the keyboard layer-by-layer
                var keyboardRect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(KeyboardScale);
                using (var g = Graphics.FromImage(frame.KeyboardBitmap))
                {
                    Profile.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Keyboard),
                        DataModel,
                        keyboardRect, true, true);
                }
                // Render mice layer-by-layer
                var devRec = new Rect(0, 0, 40, 40);
                using (var g = Graphics.FromImage(frame.MouseBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Mouse), DataModel,
                        devRec, true, true);
                }
                // Render headsets layer-by-layer
                using (var g = Graphics.FromImage(frame.HeadsetBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Headset),
                        DataModel,
                        devRec, true, true);
                }
                // Render generic devices layer-by-layer
                using (var g = Graphics.FromImage(frame.GenericBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Generic),
                        DataModel,
                        devRec, true, true);
                }
            }
        }
    }

    public class ProfilePreviewDataModel : IDataModel
    {
    }
}