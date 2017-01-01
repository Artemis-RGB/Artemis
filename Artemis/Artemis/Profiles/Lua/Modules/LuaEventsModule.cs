using System;
using System.Windows.Forms;
using Artemis.DeviceProviders;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Profiles.Lua.Modules.Events;
using Artemis.Profiles.Lua.Wrappers;
using Artemis.Utilities.Keyboard;
using MoonSharp.Interpreter;
using NLog;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaEventsModule : LuaModule
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ProfileModel _profileModel;
        public readonly string InvokeLock = string.Empty;

        public LuaEventsModule(LuaManager luaManager) : base(luaManager)
        {
            _profileModel = luaManager.ProfileModel;
            _profileModel.OnDeviceUpdatedEvent += OnDeviceUpdatedEvent;
            _profileModel.OnDeviceDrawnEvent += OnDeviceDrawnEvent;
            KeyboardHook.KeyDownCallback += KeyboardHookOnKeyDownCallback;
        }

        public override string ModuleName => "Events";

        public event EventHandler<LuaDeviceUpdatingEventArgs> DeviceUpdating;
        public event EventHandler<LuaDeviceDrawingEventArgs> DeviceDrawing;
        public event EventHandler<LuaKeyPressEventArgs> KeyboardKeyPressed;

        private void KeyboardHookOnKeyDownCallback(KeyEventArgs e)
        {
            try
            {
                var keyMatch = LuaManager.KeyboardProvider.GetKeyPosition(e.KeyCode) ?? new KeyMatch(e.KeyCode, 0, 0);
                var args = new LuaKeyPressEventArgs(e.KeyCode, keyMatch.X, keyMatch.Y);
                LuaInvoke(_profileModel, () => OnKeyboardKeyPressed(LuaManager.ProfileModule, args));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void OnDeviceUpdatedEvent(object sender, ProfileDeviceEventsArg e)
        {
            try
            {
                var args = new LuaDeviceUpdatingEventArgs(e.UpdateType, e.DataModel, e.Preview);
                LuaInvoke(_profileModel, () => OnDeviceUpdating(LuaManager.ProfileModule, args));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void OnDeviceDrawnEvent(object sender, ProfileDeviceEventsArg e)
        {
            try
            {
                var wrapper = new LuaDrawWrapper(e.DrawingContext);
                var args = new LuaDeviceDrawingEventArgs(e.UpdateType, e.DataModel, e.Preview, wrapper);
                LuaInvoke(_profileModel, () => OnDeviceDrawing(LuaManager.ProfileModule, args));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void LuaInvoke(ProfileModel profileModel, Action action)
        {
            lock (InvokeLock)
            {
                try
                {
                    action.Invoke();
                }
                catch (InternalErrorException ex)
                {
                    _logger.Error("[{0}-LUA]: Error: {1}", profileModel.Name, ex.DecoratedMessage);
                }
                catch (SyntaxErrorException ex)
                {
                    _logger.Error("[{0}-LUA]: Error: {1}", profileModel.Name, ex.DecoratedMessage);
                }
                catch (ScriptRuntimeException ex)
                {
                    _logger.Error("[{0}-LUA]: Error: {1}", profileModel.Name, ex.DecoratedMessage);
                }
            }
        }

        protected virtual void OnDeviceUpdating(LuaProfileModule profileModel, LuaDeviceUpdatingEventArgs e)
        {
            DeviceUpdating?.Invoke(profileModel, e);
        }

        protected virtual void OnDeviceDrawing(LuaProfileModule profileModel, LuaDeviceDrawingEventArgs e)
        {
            DeviceDrawing?.Invoke(profileModel, e);
        }

        protected virtual void OnKeyboardKeyPressed(LuaProfileModule profileModel, LuaKeyPressEventArgs e)
        {
            KeyboardKeyPressed?.Invoke(profileModel, e);
        }

        #region Overriding members


        public override void Dispose()
        {
            _profileModel.OnDeviceUpdatedEvent -= OnDeviceUpdatedEvent;
            _profileModel.OnDeviceDrawnEvent -= OnDeviceDrawnEvent;
            KeyboardHook.KeyDownCallback -= KeyboardHookOnKeyDownCallback;
        }

        #endregion
    }
}