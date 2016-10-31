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
        public event EventHandler<LuaProfileUpdatingEventArgs> ProfileUpdating;
        public event EventHandler<LuaProfileDrawingEventArgs> ProfileDrawing;
        public event EventHandler<LuaKeyPressEventArgs> KeyboardKeyPressed;

        internal void InvokeProfileUpdate(ProfileModel profileModel, IDataModel dataModel, bool preview)
        {
            try
            {
                OnProfileUpdating(new LuaProfileWrapper(profileModel),
                    new LuaProfileUpdatingEventArgs(dataModel, preview));
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
                OnProfileDrawing(new LuaProfileWrapper(profileModel),
                    new LuaProfileDrawingEventArgs(dataModel, preview, new LuaDrawWrapper(c)));
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
                OnKeyboardKeyPressed(new LuaProfileWrapper(profileModel), keyboard, new LuaKeyPressEventArgs(key, x, y));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected virtual void OnProfileUpdating(LuaProfileWrapper profileModel, LuaProfileUpdatingEventArgs e)
        {
            try
            {
                ProfileUpdating?.Invoke(profileModel, e);
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

        protected virtual void OnProfileDrawing(LuaProfileWrapper profileModel, LuaProfileDrawingEventArgs e)
        {
            try
            {
                ProfileDrawing?.Invoke(profileModel, e);
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

        protected virtual void OnKeyboardKeyPressed(LuaProfileWrapper profileModel, LuaKeyboardWrapper keyboard,
            LuaKeyPressEventArgs e)
        {
            try
            {
                KeyboardKeyPressed?.Invoke(profileModel, e);
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
}