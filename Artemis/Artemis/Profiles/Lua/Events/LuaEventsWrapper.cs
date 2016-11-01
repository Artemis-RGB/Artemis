using System;
using System.Windows.Forms;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;
using NLog;

namespace Artemis.Profiles.Lua.Events
{
    [MoonSharpUserData]
    public class LuaEventsWrapper
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _invokeLock = string.Empty;
        public event EventHandler<LuaProfileUpdatingEventArgs> ProfileUpdating;
        public event EventHandler<LuaProfileDrawingEventArgs> ProfileDrawing;
        public event EventHandler<LuaKeyPressEventArgs> KeyboardKeyPressed;

        internal void InvokeProfileUpdate(ProfileModel profileModel, IDataModel dataModel, bool preview)
        {
            try
            {
                LuaInvoke(profileModel, () => OnProfileUpdating(new LuaProfileWrapper(profileModel),
                    new LuaProfileUpdatingEventArgs(dataModel, preview)));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal void InvokeProfileDraw(ProfileModel profileModel, IDataModel dataModel, bool preview, DrawingContext c)
        {
            try
            {
                LuaInvoke(profileModel, () => OnProfileDrawing(new LuaProfileWrapper(profileModel),
                    new LuaProfileDrawingEventArgs(dataModel, preview, new LuaDrawWrapper(c))));
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
            lock (_invokeLock)
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

        protected virtual void OnProfileUpdating(LuaProfileWrapper profileModel, LuaProfileUpdatingEventArgs e)
        {
            ProfileUpdating?.Invoke(profileModel, e);
        }

        protected virtual void OnProfileDrawing(LuaProfileWrapper profileModel, LuaProfileDrawingEventArgs e)
        {
            ProfileDrawing?.Invoke(profileModel, e);
        }

        protected virtual void OnKeyboardKeyPressed(LuaProfileWrapper profileModel, LuaKeyboardWrapper keyboard,
            LuaKeyPressEventArgs e)
        {
            KeyboardKeyPressed?.Invoke(profileModel, e);
        }
    }
}