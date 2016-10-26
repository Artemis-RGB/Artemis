using System;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;
using NLog;

namespace Artemis.Profiles.Lua
{
    [MoonSharpUserData]
    public class LuaEventsWrapper
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public event EventHandler<LuaProfileUpdatingEventArgs> LuaProfileUpdating;
        public event EventHandler<LuaProfileDrawingEventArgs> LuaProfileDrawing;

        internal void InvokeProfileUpdate(ProfileModel profileModel, IDataModel dataModel, bool preview)
        {
            OnLuaProfileUpdating(new LuaProfileWrapper(profileModel),
                new LuaProfileUpdatingEventArgs(dataModel, preview));
        }

        internal void InvokeProfileDraw(ProfileModel profileModel, IDataModel dataModel, bool preview, DrawingContext c)
        {
            OnLuaProfileDrawing(new LuaProfileWrapper(profileModel),
                new LuaProfileDrawingEventArgs(dataModel, preview, new LuaDrawWrapper(c)));
        }

        
        protected virtual void OnLuaProfileUpdating(LuaProfileWrapper profileModel, LuaProfileUpdatingEventArgs e)
        {
            try
            {
                LuaProfileUpdating?.Invoke(profileModel, e);
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

        protected virtual void OnLuaProfileDrawing(LuaProfileWrapper profileModel, LuaProfileDrawingEventArgs e)
        {
            try
            {
                LuaProfileDrawing?.Invoke(profileModel, e);
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

    [MoonSharpUserData]
    public class LuaProfileUpdatingEventArgs : EventArgs
    {
        public LuaProfileUpdatingEventArgs(IDataModel dataModel, bool preview)
        {
            DataModel = dataModel;
            Preview = preview;
        }

        public IDataModel DataModel { get; }
        public bool Preview { get; }
    }

    [MoonSharpUserData]
    public class LuaProfileDrawingEventArgs : EventArgs
    {
        public LuaProfileDrawingEventArgs(IDataModel dataModel, bool preview, LuaDrawWrapper luaDrawWrapper)
        {
            DataModel = dataModel;
            Preview = preview;
            Drawing = luaDrawWrapper;
        }

        public IDataModel DataModel { get; }
        public bool Preview { get; }
        public LuaDrawWrapper Drawing { get; set; }
    }
}