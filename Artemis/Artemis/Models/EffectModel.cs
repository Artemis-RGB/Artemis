using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using Artemis.DAL;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Settings;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.Models
{
    public abstract class EffectModel : IDisposable
    {
        public delegate void SettingsUpdateHandler(EffectSettings settings);

        protected DateTime LastTrace;

        protected EffectModel(DeviceManager deviceManager, EffectSettings settings, IDataModel dataModel)
        {
            DeviceManager = deviceManager;
            Settings = settings;
            DataModel = dataModel;

            // If set, load the last profile from settings
            if (!string.IsNullOrEmpty(Settings?.LastProfile))
                Profile = ProfileProvider.GetProfile(DeviceManager.ActiveKeyboard, this, Settings.LastProfile);

            DeviceManager.OnKeyboardChangedEvent += DeviceManagerOnOnKeyboardChangedEvent;
        }

        private void DeviceManagerOnOnKeyboardChangedEvent(object sender, KeyboardChangedEventArgs args)
        {
            if (!string.IsNullOrEmpty(Settings?.LastProfile))
                Profile = ProfileProvider.GetProfile(DeviceManager.ActiveKeyboard, this, Settings.LastProfile);
        }

        public bool Initialized { get; set; }
        public DeviceManager DeviceManager { get; set; }
        public EffectSettings Settings { get; set; }
        public string Name { get; set; }
        public int KeyboardScale { get; set; } = 4;
        // Used by profile system
        public IDataModel DataModel { get; set; }
        public ProfileModel Profile { get; set; }

        [Inject]
        public ILogger Logger { get; set; }

        public virtual void Dispose()
        {
            Profile?.Deactivate();
        }

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
            if ((Profile == null) || (DataModel == null) || (DeviceManager.ActiveKeyboard == null))
                return;

            lock (DataModel)
            {
                // Get all enabled layers who's conditions are met
                var renderLayers = GetRenderLayers(keyboardOnly);

                // If the profile has no active LUA wrapper, create one
                if (!string.IsNullOrEmpty(Profile.LuaScript))
                    Profile.Activate(DeviceManager.ActiveKeyboard);

                // Render the keyboard layer-by-layer
                var keyboardRect = DeviceManager.ActiveKeyboard.KeyboardRectangle(KeyboardScale);
                using (var g = Graphics.FromImage(frame.KeyboardBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Keyboard),
                        DataModel, keyboardRect, false, true, "keyboard");
                }
                // Render mice layer-by-layer
                var devRec = new Rect(0, 0, 40, 40);
                using (var g = Graphics.FromImage(frame.MouseBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Mouse), DataModel,
                        devRec, false, true, "mouse");
                }
                // Render headsets layer-by-layer
                using (var g = Graphics.FromImage(frame.HeadsetBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Headset),
                        DataModel, devRec, false, true, "headset");
                }
                // Render generic devices layer-by-layer
                using (var g = Graphics.FromImage(frame.GenericBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Generic),
                        DataModel, devRec, false, true, "generic");
                }
                // Render mousemats layer-by-layer
                using (var g = Graphics.FromImage(frame.MousematBitmap))
                {
                    Profile?.DrawLayers(g, renderLayers.Where(rl => rl.LayerType.DrawType == DrawType.Mousemat),
                        DataModel, devRec, false, true, "mousemat");
                }

                // Trace debugging
                if (DateTime.Now.AddSeconds(-2) <= LastTrace || Logger == null)
                    return;

                LastTrace = DateTime.Now;
                var dmJson = JsonConvert.SerializeObject(DataModel, Formatting.Indented);
                Logger.Trace("Effect datamodel as JSON: \r\n{0}", dmJson);
                Logger.Trace("Effect {0} has to render {1} layers", Name, renderLayers.Count);
                foreach (var renderLayer in renderLayers)
                    Logger.Trace("- Layer name: {0}, layer type: {1}", renderLayer.Name, renderLayer.LayerType);
            }
        }

        public abstract List<LayerModel> GetRenderLayers(bool keyboardOnly);
    }
}