using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.DeviceProviders;
using Artemis.Profiles;
using Artemis.Profiles.Lua;
using Artemis.Profiles.Lua.Modules;
using Castle.Core.Internal;
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
        private readonly Script _luaScript;
        private List<LuaModule> _luaModules;

        public LuaManager(IKernel kernel, ILogger logger, DeviceManager deviceManager)
        {
            _kernel = kernel;
            _logger = logger;
            _deviceManager = deviceManager;
            _luaScript = new Script(CoreModules.Preset_SoftSandbox);
        }

        public ProfileModel ProfileModel { get; private set; }
        public KeyboardProvider KeyboardProvider { get; private set; }
        public LuaProfileModule ProfileModule { get; private set; }
        public LuaEventsModule EventsModule { get; private set; }

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
            _luaScript.Options.DebugPrint = LuaPrint;

            // Insert each module into the script's globals
            foreach (var luaModule in _luaModules)
                _luaScript.Globals[luaModule.ModuleName] = luaModule;

            // If there is no LUA script, don't bother executing the string
            if (ProfileModel.LuaScript.IsNullOrEmpty())
                return;

            try
            {
                lock (EventsModule.InvokeLock)
                {
                    lock (_luaScript)
                    {
                        _luaScript.DoString(ProfileModel.LuaScript);
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
                _luaScript.Globals.Clear();
                _luaScript.Registry.Clear();
                _luaScript.Registry.RegisterConstants();
                _luaScript.Registry.RegisterCoreModules(CoreModules.Preset_SoftSandbox);
                _luaScript.Globals.RegisterConstants();
                _luaScript.Globals.RegisterCoreModules(CoreModules.Preset_SoftSandbox);
            }
            catch (NullReferenceException)
            {
                // TODO: Ask MoonSharp folks why this is happening
            }

            if (EventsModule != null)
                lock (EventsModule.InvokeLock)
                {
                    lock (_luaScript)
                    {
                        _luaScript.DoString("");
                    }
                }
            else
                lock (_luaScript)
                {
                    _luaScript.DoString("");
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
                    lock (_luaScript)
                    {
                        if (args != null)
                            _luaScript.Call(function, args);
                        else
                            _luaScript.Call(function);
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