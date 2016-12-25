using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Artemis.DAL;
using Artemis.DeviceProviders;
using Artemis.Profiles;
using Artemis.Profiles.Lua;
using Artemis.Profiles.Lua.Modules;
using Artemis.Properties;
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
        private List<LuaModule> _luaModules;
        private readonly Script _luaScript;
        private FileSystemWatcher _watcher;

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
            // Clear old state
            ClearLua();

            // Stop after that if no model provided/there is no keyboard
            if (profileModel == null || _deviceManager.ActiveKeyboard == null)
                return;

            ProfileModel = profileModel;
            KeyboardProvider = _deviceManager.ActiveKeyboard;

            // Get new instances of all modules
            _luaModules = _kernel.Get<List<LuaModule>>();
            ProfileModule = (LuaProfileModule)_luaModules.First(m => m.ModuleName == "Profile");
            EventsModule = (LuaEventsModule)_luaModules.First(m => m.ModuleName == "Events");

            // Setup new state
            _luaScript.Options.DebugPrint = LuaPrint;

            // Insert each module into the script's globals
            foreach (var luaModule in _luaModules)
            {                
                _luaScript.Globals[luaModule.ModuleName] = luaModule;
            }

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
                _logger.Error(e, "[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (SyntaxErrorException e)
            {
                _logger.Error(e, "[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (ScriptRuntimeException e)
            {
                _logger.Error(e, "[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
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
            {
                lock (EventsModule.InvokeLock)
                {
                    lock (_luaScript)
                    {
                        _luaScript.DoString("");
                    }
                }
            }
            else
            {
                lock (_luaScript)
                {
                    _luaScript.DoString("");
                }
            }
        }

        #region Private lua functions

        private void LuaPrint(string s)
        {
            _logger.Info("[{0}-LUA]: {1}", ProfileModel?.Name, s);
        }

        #endregion

        #region Editor

        public void OpenEditor()
        {
            if (ProfileModel == null)
                return;

            // Create a temp file
            var fileName = Guid.NewGuid() + ".lua";
            var file = File.Create(Path.GetTempPath() + fileName);
            file.Dispose();

            // Add instructions to LUA script if it's a new file
            if (ProfileModel.LuaScript.IsNullOrEmpty())
                ProfileModel.LuaScript = Encoding.UTF8.GetString(Resources.lua_placeholder);
            File.WriteAllText(Path.GetTempPath() + fileName, ProfileModel.LuaScript);

            // Watch the file for changes
            SetupWatcher(Path.GetTempPath(), fileName);

            // Open the temp file with the default editor
            System.Diagnostics.Process.Start(Path.GetTempPath() + fileName);
        }

        private void SetupWatcher(string path, string fileName)
        {
            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher(Path.GetTempPath(), fileName);
                _watcher.Changed += LuaFileChanged;
                _watcher.EnableRaisingEvents = true;
            }

            _watcher.Path = path;
            _watcher.Filter = fileName;
        }

        private void LuaFileChanged(object sender, FileSystemEventArgs args)
        {
            if (args.ChangeType != WatcherChangeTypes.Changed)
                return;

            if (ProfileModel == null)
                return;

            lock (ProfileModel)
            {
                using (var fs = new FileStream(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        ProfileModel.LuaScript = sr.ReadToEnd();
                    }
                }

                ProfileProvider.AddOrUpdate(ProfileModel);
                SetupLua(ProfileModel);
            }
        }

        #endregion
    }
}