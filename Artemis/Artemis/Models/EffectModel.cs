using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Layers.Types;
using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Newtonsoft.Json;
using Brush = System.Windows.Media.Brush;

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
        public virtual void Render(Graphics keyboard, out Brush mouse, out Brush headset, bool renderMice,
            bool renderHeadsets)
        {
            mouse = null;
            headset = null;

            if (Profile == null || DataModel == null || MainManager.DeviceManager.ActiveKeyboard == null)
                return;

            // Get all enabled layers who's conditions are met
            var renderLayers = GetRenderLayers(renderMice, renderHeadsets);

            // Render the keyboard layer-by-layer
            Profile.DrawProfile(keyboard, renderLayers, DataModel,
                MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(KeyboardScale), false, true);
            // Render the first enabled mouse (will default to null if renderMice was false)
            mouse = Profile.GenerateBrush(renderLayers.LastOrDefault(l => l.LayerType is MouseType), DataModel);
            // Render the first enabled headset (will default to null if renderHeadsets was false)
            headset = Profile.GenerateBrush(renderLayers.LastOrDefault(l => l.LayerType is HeadsetType), DataModel);

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