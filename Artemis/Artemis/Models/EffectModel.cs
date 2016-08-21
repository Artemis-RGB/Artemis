using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Settings;
using Newtonsoft.Json;

namespace Artemis.Models
{
    public abstract class EffectModel : IDisposable
    {
        public delegate void SettingsUpdateHandler(EffectSettings settings);

        protected DateTime LastTrace;

        protected EffectModel(MainManager mainManager, EffectSettings settings, IDataModel dataModel)
        {
            MainManager = mainManager;
            Settings = settings;
            DataModel = dataModel;

            MainManager.EffectManager.EffectModels.Add(this);
        }

        public bool Initialized { get; set; }
        public MainManager MainManager { get; set; }
        public EffectSettings Settings { get; set; }
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
        /// <summary>
        ///     Renders the currently active profile
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="keyboardOnly"></param>
        public virtual void Render(RenderFrame frame, bool keyboardOnly)
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
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Keyboard),
                        DataModel, keyboardRect, false, true);
                }
                // Render mice layer-by-layer
                var devRec = new Rect(0, 0, 40, 40);
                using (var g = Graphics.FromImage(frame.MouseBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Mouse), DataModel,
                        devRec, false, true);
                }
                // Render headsets layer-by-layer
                using (var g = Graphics.FromImage(frame.HeadsetBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Headset),
                        DataModel, devRec, false, true);
                }
                // Render generic devices layer-by-layer
                using (var g = Graphics.FromImage(frame.GenericBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Generic),
                        DataModel, devRec, false, true);
                }

                // Trace debugging
                if (DateTime.Now.AddSeconds(-2) <= LastTrace)
                    return;
                LastTrace = DateTime.Now;
                MainManager.Logger.Trace("Effect datamodel as JSON: \r\n{0}",
                    JsonConvert.SerializeObject(DataModel, Formatting.Indented));
                MainManager.Logger.Trace("Effect {0} has to render {1} layers", Name, renderLayers.Count);
                foreach (var renderLayer in renderLayers)
                    MainManager.Logger.Trace("- Layer name: {0}, layer type: {1}", renderLayer.Name,
                        renderLayer.LayerType);
            }
        }

        public abstract List<LayerModel> GetRenderLayers(bool keyboardOnly);
    }
}