using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Models
{
    public abstract class EffectModel : IDisposable
    {
        public delegate void SettingsUpdateHandler(EffectSettings settings);

        public bool Initialized;
        public MainManager MainManager;
        public string Name;

        protected EffectModel(MainManager mainManager, IDataModel dataModel)
        {
            MainManager = mainManager;
            DataModel = dataModel;
        }

        public abstract void Dispose();

        // Called on creation
        public abstract void Enable();

        // Called every frame
        public abstract void Update();

        // Used by profile system
        public IDataModel DataModel { get; set; }
        public ProfileModel Profile { get; set; }

        // Called after every update
        public virtual void Render(out Bitmap keyboard, out Brush mouse, out Brush headset, bool renderMice, bool renderHeadsets)
        {
            keyboard = null;
            mouse = null;
            headset = null;

            if (Profile == null || DataModel == null)
                return;

            // Get all enabled layers who's conditions are met
            var renderLayers = GetRenderLayers(renderMice, renderHeadsets);

            // Render the keyboard layer-by-layer
            keyboard = Profile.GenerateBitmap(renderLayers, DataModel, MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(4), false, true);
            // Render the first enabled mouse (will default to null if renderMice was false)
            mouse = Profile.GenerateBrush(renderLayers.LastOrDefault(l => l.LayerType == LayerType.Mouse), DataModel);
            // Render the first enabled headset (will default to null if renderHeadsets was false)
            headset = Profile.GenerateBrush(renderLayers.LastOrDefault(l => l.LayerType == LayerType.Headset), DataModel);
        }

        public abstract List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets);
    }
}