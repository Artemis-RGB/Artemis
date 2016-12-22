using System;
using System.Windows.Forms;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Lua.Modules;
using MoonSharp.Interpreter;
using NLog;

namespace Artemis.Profiles.Lua.Events
{
    [MoonSharpUserData]
    public class LuaEventsWrapper
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public readonly string InvokeLock = string.Empty;
        public event EventHandler<LuaDeviceUpdatingEventArgs> DeviceUpdating;
        public event EventHandler<LuaDeviceDrawingEventArgs> DeviceDrawing;
        public event EventHandler<LuaKeyPressEventArgs> KeyboardKeyPressed;

        internal void InvokeDeviceUpdate(ProfileModel profileModel, string deviceType, IDataModel dataModel,
            bool preview)
        {
            try
            {
                LuaInvoke(profileModel, () => OnDeviceUpdating(new LuaProfileWrapper(profileModel),
                    new LuaDeviceUpdatingEventArgs(deviceType, dataModel, preview)));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal void InvokeDeviceDraw(ProfileModel profileModel, string deviceType, IDataModel dataModel, bool preview,
            DrawingContext c)
        {
            try
            {
                LuaInvoke(profileModel, () => OnDeviceDrawing(new LuaProfileWrapper(profileModel),
                    new LuaDeviceDrawingEventArgs(deviceType, dataModel, preview, new LuaDrawModule(c))));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal void InvokeKeyPressed(ProfileModel profileModel, LuaKeyboardWrapper keyboard, Keys key, int x, int y)
        {
            try
            {
                LuaInvoke(profileModel, () => OnKeyboardKeyPressed(new LuaProfileWrapper(profileModel),
                    keyboard, new LuaKeyPressEventArgs(key, x, y)));
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
                    _logger.Error(ex, "[{0}-LUA]: Error: {1}", profileModel.Name, ex.DecoratedMessage);
                }
                catch (SyntaxErrorException ex)
                {
                    _logger.Error(ex, "[{0}-LUA]: Error: {1}", profileModel.Name, ex.DecoratedMessage);
                }
                catch (ScriptRuntimeException ex)
                {
                    _logger.Error(ex, "[{0}-LUA]: Error: {1}", profileModel.Name, ex.DecoratedMessage);
                }
            }
        }

        protected virtual void OnDeviceUpdating(LuaProfileWrapper profileModel, LuaDeviceUpdatingEventArgs e)
        {
            DeviceUpdating?.Invoke(profileModel, e);
        }

        protected virtual void OnDeviceDrawing(LuaProfileWrapper profileModel, LuaDeviceDrawingEventArgs e)
        {
            DeviceDrawing?.Invoke(profileModel, e);
        }

        protected virtual void OnKeyboardKeyPressed(LuaProfileWrapper profileModel, LuaKeyboardWrapper keyboard,
            LuaKeyPressEventArgs e)
        {
            KeyboardKeyPressed?.Invoke(profileModel, e);
        }
    }
}