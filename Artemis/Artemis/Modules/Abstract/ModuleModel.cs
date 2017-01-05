using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Artemis.DAL;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.Modules.Abstract
{
    public abstract class ModuleModel : IDisposable
    {
        private readonly LuaManager _luaManager;
        protected readonly DeviceManager DeviceManager;
        private DateTime _lastTrace;

        public ModuleModel(DeviceManager deviceManager, LuaManager luaManager)
        {
            _luaManager = luaManager;
            DeviceManager = deviceManager;

            DeviceManager.OnKeyboardChanged += OnKeyboardChanged;
        }

        #region Events

        public event EventHandler<ProfileChangedEventArgs> ProfileChanged;

        #endregion

        #region Abstract properties

        /// <summary>
        ///     The module's name, used in setting files and logging
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Whether or not the module should be rendered as an overlay
        /// </summary>
        public abstract bool IsOverlay { get; }

        /// <summary>
        ///     Whether or not the module is enabled by a certain process
        /// </summary>
        public abstract bool IsBoundToProcess { get; }

        /// <summary>
        ///     The module's settings
        /// </summary>
        public ModuleSettings Settings { get; set; }

        /// <summary>
        ///     The module's data model
        /// </summary>
        public ModuleDataModel DataModel { get; set; }

        #endregion

        #region Base properties

        [Inject]
        public ILogger Logger { get; set; }

        /// <summary>
        ///     Whether or not the module is initialized and ready to be updated/rendered
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        ///     If this collection contains any layers they will be drawn instead of the regular profile
        /// </summary>
        public List<LayerModel> PreviewLayers { get; set; }

        /// <summary>
        ///     The process the module is bound to
        /// </summary>
        public string ProcessName { get; protected set; }

        /// <summary>
        ///     The currently active profile of the module
        /// </summary>
        public ProfileModel ProfileModel { get; protected set; }

        #endregion

        #region Base methods

        public void ChangeProfile(ProfileModel profileModel)
        {
            if (!IsInitialized || Equals(profileModel, ProfileModel))
                return;

            ProfileModel = profileModel;
            ProfileModel?.Activate(_luaManager);

            RaiseProfileChangedEvent(new ProfileChangedEventArgs(ProfileModel));
        }

        public virtual void Enable()
        {
            IsInitialized = true;
            ChangeToLastProfile();
        }

        public virtual void Dispose()
        {
            IsInitialized = false;

            PreviewLayers = null;
            ProfileModel?.Deactivate(_luaManager);
            ProfileModel = null;
        }

        private void OnKeyboardChanged(object sender, KeyboardChangedEventArgs e)
        {
            ChangeToLastProfile();
        }

        private void ChangeToLastProfile()
        {
            var profileName = !string.IsNullOrEmpty(Settings?.LastProfile) ? Settings.LastProfile : "Default";
            ChangeProfile(ProfileProvider.GetProfile(DeviceManager.ActiveKeyboard, this, profileName));
        }

        protected virtual void RaiseProfileChangedEvent(ProfileChangedEventArgs e)
        {
            var handler = ProfileChanged;
            handler?.Invoke(this, e);
        }

        public abstract void Update();

        public virtual void Render(RenderFrame frame, bool keyboardOnly)
        {
            if (ProfileModel == null || DataModel == null || DeviceManager.ActiveKeyboard == null)
                return;

            lock (DataModel)
            {
                lock (ProfileModel)
                {
                    // Use the preview layers if they are present, else get all layers who's conditions are met
                    var layers = PreviewLayers ?? GetRenderLayers(keyboardOnly);
                    var preview = PreviewLayers != null;

                    // Render the keyboard layer-by-layer
                    var keyboardRect = DeviceManager.ActiveKeyboard.KeyboardRectangle();
                    using (var g = Graphics.FromImage(frame.KeyboardBitmap))
                    {
                        ProfileModel?.DrawLayers(g, layers, DrawType.Keyboard, DataModel, keyboardRect, preview);
                    }
                    // Render mice layer-by-layer
                    var devRec = new Rect(0, 0, 40, 40);
                    using (var g = Graphics.FromImage(frame.MouseBitmap))
                    {
                        ProfileModel?.DrawLayers(g, layers, DrawType.Mouse, DataModel, devRec, preview);
                    }
                    // Render headsets layer-by-layer
                    using (var g = Graphics.FromImage(frame.HeadsetBitmap))
                    {
                        ProfileModel?.DrawLayers(g, layers, DrawType.Headset, DataModel, devRec, preview);
                    }
                    // Render generic devices layer-by-layer
                    using (var g = Graphics.FromImage(frame.GenericBitmap))
                    {
                        ProfileModel?.DrawLayers(g, layers, DrawType.Generic, DataModel, devRec, preview);
                    }
                    // Render mousemats layer-by-layer
                    using (var g = Graphics.FromImage(frame.MousematBitmap))
                    {
                        ProfileModel?.DrawLayers(g, layers, DrawType.Mousemat, DataModel, devRec, preview);
                    }

                    // Trace debugging
                    if (DateTime.Now.AddSeconds(-2) <= _lastTrace || Logger == null)
                        return;

                    _lastTrace = DateTime.Now;
                    var dmJson = JsonConvert.SerializeObject(DataModel, Formatting.Indented);
                    Logger.Trace("Effect datamodel as JSON: \r\n{0}", dmJson);
                    Logger.Trace("Effect {0} has to render {1} layers", Name, layers.Count);
                    foreach (var renderLayer in layers)
                        Logger.Trace("- Layer name: {0}, layer type: {1}", renderLayer.Name, renderLayer.LayerType);
                }
            }
        }

        public virtual List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return ProfileModel?.GetRenderLayers(DataModel, keyboardOnly);
        }

        #endregion
    }
}