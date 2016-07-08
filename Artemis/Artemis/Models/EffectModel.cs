using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Headset;
using Artemis.Profiles.Layers.Types.Mouse;
using Newtonsoft.Json;

namespace Artemis.Models
{
    public abstract class EffectModel : IDisposable
    {
        public delegate void SettingsUpdateHandler(EffectSettings settings);

        protected DateTime LastTrace;

        protected EffectModel(MainManager mainManager, IDataModel dataModel)
        {
            MainManager = mainManager;
            DataModel = dataModel;
        }

        public bool Initialized { get; set; }
        public MainManager MainManager { get; set; }
        public string Name { get; set; }
        public int KeyboardScale { get; set; } = 4;

        // Used by profile system
        public IDataModel DataModel { get; set; }
        public ProfileModel Profile { get; set; }

        public abstract void Dispose();

        // Called on creation
        public abstract void Enable();

        // Called every frame
        public abstract void Update();

        // Called after every update
        public virtual void Render(Bitmap keyboard, Bitmap mouse, Bitmap headset, bool renderMice, bool renderHeadsets)
        {
            if (Profile == null || DataModel == null || MainManager.DeviceManager.ActiveKeyboard == null)
                return;

            // Get all enabled layers who's conditions are met
            var renderLayers = GetRenderLayers(renderMice, renderHeadsets);

            // Render the keyboard layer-by-layer
            var keyboardRect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(KeyboardScale);
            using (var g = Graphics.FromImage(keyboard))
            {
                Profile.DrawLayers(g, renderLayers.Where(rl => rl.MustDraw()), DataModel, keyboardRect, false, true);
            }

            // Render the mouse layer-by-layer
            var smallRect = new Rect(0, 0, 40, 40);
            using (var g = Graphics.FromImage(mouse))
            {
                Profile.DrawLayers(g, renderLayers.Where(rl => rl.LayerType is MouseType), DataModel, smallRect,
                    false, true);
            }

            // Render the headset layer-by-layer
            using (var g = Graphics.FromImage(headset))
            {
                Profile.DrawLayers(g, renderLayers.Where(rl => rl.LayerType is HeadsetType), DataModel, smallRect,
                    false, true);
            }

            // Trace debugging
            if (DateTime.Now.AddSeconds(-2) <= LastTrace)
                return;
            LastTrace = DateTime.Now;
            MainManager.Logger.Trace("Effect datamodel as JSON: \r\n{0}",
                JsonConvert.SerializeObject(DataModel, Formatting.Indented));
            MainManager.Logger.Trace("Effect {0} has to render {1} layers", Name, renderLayers.Count);
            foreach (var renderLayer in renderLayers)
                MainManager.Logger.Trace("- Layer name: {0}, layer type: {1}", renderLayer.Name, renderLayer.LayerType);
        }

        public abstract List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets);
    }
}