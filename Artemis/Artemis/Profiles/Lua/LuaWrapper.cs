using System;
using System.IO;
using System.Text;
using Artemis.DAL;
using Artemis.DeviceProviders;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Lua.Brushes;
using Artemis.Profiles.Lua.Modules.Events;
using Artemis.Properties;
using Castle.Core.Internal;
using MoonSharp.Interpreter;
using Ninject;
using NLog;

namespace Artemis.Profiles.Lua
{
    /// <summary>
    /// This class is a singleton due to the fact that the LuaScript isn't very memory
    /// friendly when creating new ones over and over.
    /// </summary>
    public class LuaWrapper
    {
        private readonly IKernel _kernel;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Script _luaScript = new Script(CoreModules.Preset_SoftSandbox);
        private FileSystemWatcher _watcher;

        public ProfileModel ProfileModel { get; private set; }
        public KeyboardProvider KeyboardProvider { get; private set; }
        public LayerModel LayerModel { get; set; }

        public LuaWrapper(IKernel kernel)
        {
            _kernel = kernel;
        }

        public static void SetupLua(ProfileModel profileModel, KeyboardProvider keyboardProvider)
        {
            Clear();

            if ((profileModel == null) || (keyboardProvider == null))
                return;

            // Setup a new environment
            KeyboardProvider = keyboardProvider;
            ProfileModel = profileModel;
            LuaProfileWrapper = new LuaProfileWrapper(ProfileModel);
            LuaBrushWrapper = new LuaBrushWrapper();
            LuaKeyboardWrapper = new LuaKeyboardWrapper(keyboardProvider);
            LuaMouseWrapper = new LuaMouseWrapper();
            LuaEventsWrapper = new LuaEventsWrapper();

            _luaScript.Options.DebugPrint = LuaPrint;
            _luaScript.Globals["Profile"] = LuaProfileWrapper;
            _luaScript.Globals["Events"] = LuaEventsWrapper;
            _luaScript.Globals["Brushes"] = LuaBrushWrapper;
            _luaScript.Globals["Keyboard"] = LuaKeyboardWrapper;
            _luaScript.Globals["Mouse"] = LuaMouseWrapper;

            if (ProfileModel == null)
                return;
            if (ProfileModel.LuaScript.IsNullOrEmpty())
                return;

            try
            {
                lock (LuaEventsWrapper.InvokeLock)
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

        public static void Clear()
        {
            lock (_luaScript)
            {
                // Clear old fields/properties
                KeyboardProvider = null;
                ProfileModel = null;
                LuaProfileWrapper = null;
                LuaBrushWrapper = null;
                LuaKeyboardWrapper?.Dispose();
                LuaKeyboardWrapper = null;
                LuaMouseWrapper = null;

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

                if (LuaEventsWrapper != null)
                {
                    lock (LuaEventsWrapper.InvokeLock)
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


                LuaEventsWrapper = null;
            }
        }

        #region Private lua functions

        private static void LuaPrint(string s)
        {
            _logger.Debug("[{0}-LUA]: {1}", ProfileModel?.Name, s);
        }

        #endregion

        #region Editor

        public static void OpenEditor()
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

        private static void SetupWatcher(string path, string fileName)
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

        private static void LuaFileChanged(object sender, FileSystemEventArgs args)
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

                if (KeyboardProvider != null)
                    SetupLua(ProfileModel, KeyboardProvider);
            }
        }

        #endregion
    }
}