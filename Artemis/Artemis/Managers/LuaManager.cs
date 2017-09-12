using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.DeviceProviders;
using Artemis.Profiles;
using Artemis.Profiles.Lua;
using Artemis.Profiles.Lua.Modules;
using Artemis.Profiles.Lua.Modules.Gui;
using MoonSharp.Interpreter;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis.Managers
{
    public class LuaManager
    {
        private readonly DeviceManager _deviceManager;
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private List<LuaModule> _luaModules;

        public LuaManager(IKernel kernel, ILogger logger, DeviceManager deviceManager)
        {
            _kernel = kernel;
            _logger = logger;
            _deviceManager = deviceManager;

            EditorButtons = new ObservableCollection<EditorButton>();
            LuaScript = new Script(CoreModules.Preset_SoftSandbox);
        }

        public ProfileModel ProfileModel { get; private set; }
        public KeyboardProvider KeyboardProvider { get; private set; }
        public LuaProfileModule ProfileModule { get; private set; }
        public LuaEventsModule EventsModule { get; private set; }
        public Script LuaScript { get; }
        public ObservableCollection<EditorButton> EditorButtons { get; set; }

        public void SetupLua(ProfileModel profileModel)
        {
            _logger.Debug("Setting up LUA for profile '{0}', module '{1}'", profileModel?.Name, profileModel?.GameName);
            // Clear old state
            ClearLua();

            // Stop after that if no model provided/there is no keyboard
            if (profileModel == null || _deviceManager.ActiveKeyboard == null)
                return;

            ProfileModel = profileModel;
            KeyboardProvider = _deviceManager.ActiveKeyboard;

            // Get new instances of all modules
            _luaModules = _kernel.Get<List<LuaModule>>();
            ProfileModule = (LuaProfileModule) _luaModules.First(m => m.ModuleName == "Profile");
            EventsModule = (LuaEventsModule) _luaModules.First(m => m.ModuleName == "Events");

            // Setup new state
            LuaScript.Options.DebugPrint = LuaPrint;

            // Insert each module with a ModuleName into the script's globals
            foreach (var luaModule in _luaModules.Where(m => m.ModuleName != null))
                LuaScript.Globals[luaModule.ModuleName] = luaModule;

            // If there is no LUA script, don't bother executing the string
            if (string.IsNullOrEmpty(ProfileModel.LuaScript))
                return;

            try
            {
                lock (EventsModule.InvokeLock)
                {
                    lock (LuaScript)
                    {
                        LuaScript.DoString(ProfileModel.LuaScript);
                    }
                }
            }
            catch (InternalErrorException e)
            {
                _logger.Error("[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (SyntaxErrorException e)
            {
                _logger.Error("[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (ScriptRuntimeException e)
            {
                _logger.Error("[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
        }

        public void ClearLua()
        {
            if (_luaModules != null)
            {
                foreach (var luaModule in _luaModules)
                    luaModule.Dispose();
                _luaModules.Clear();
            }

            try
            {
                LuaScript.Globals.Clear();
                LuaScript.Registry.Clear();
                LuaScript.Registry.RegisterConstants();
                LuaScript.Registry.RegisterCoreModules(CoreModules.Preset_SoftSandbox);
                LuaScript.Globals.RegisterConstants();
                LuaScript.Globals.RegisterCoreModules(CoreModules.Preset_SoftSandbox);
            }
            catch (NullReferenceException)
            {
                // TODO: Ask MoonSharp folks why this is happening
            }

            if (EventsModule != null)
                lock (EventsModule.InvokeLock)
                {
                    lock (LuaScript)
                    {
                        LuaScript.DoString("");
                    }
                }
            else
                lock (LuaScript)
                {
                    LuaScript.DoString("");
                }
        }

        /// <summary>
        ///     Safely call a function on the active script
        /// </summary>
        /// <param name="function"></param>
        /// <param name="args"></param>
        public void Call(DynValue function, DynValue[] args = null)
        {
            if (EventsModule == null)
                return;

            try
            {
                lock (EventsModule.InvokeLock)
                {
                    lock (LuaScript)
                    {
                        if (args != null)
                            LuaScript.Call(function, args);
                        else
                            LuaScript.Call(function);
                    }
                }
            }
            catch (ArgumentException e)
            {
                _logger.Error("[{0}-LUA]: Error: {1}", ProfileModel.Name, e.Message);
            }
            catch (InternalErrorException e)
            {
                _logger.Error("[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (SyntaxErrorException e)
            {
                _logger.Error("[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (ScriptRuntimeException e)
            {
                _logger.Error("[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
        }

        #region Private lua functions

        private void LuaPrint(string s)
        {
            _logger.Info("[{0}-LUA]: {1}", ProfileModel?.Name, s);
        }

        #endregion
    }
}