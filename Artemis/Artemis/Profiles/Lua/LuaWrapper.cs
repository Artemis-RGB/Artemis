using System;
using System.IO;
using System.Text;
using Artemis.DAL;
using Artemis.DeviceProviders;
using Artemis.Profiles.Lua.Brushes;
using Artemis.Profiles.Lua.Events;
using Artemis.Properties;
using Castle.Core.Internal;
using MoonSharp.Interpreter;
using NLog;

namespace Artemis.Profiles.Lua
{
    public static class LuaWrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Script LuaScript = new Script(CoreModules.Preset_SoftSandbox);
        private static FileSystemWatcher _watcher;

        public static ProfileModel ProfileModel { get; private set; }
        public static KeyboardProvider KeyboardProvider { get; private set; }
        public static LuaProfileWrapper LuaProfileWrapper { get; private set; }
        public static LuaBrushWrapper LuaBrushWrapper { get; private set; }
        public static LuaKeyboardWrapper LuaKeyboardWrapper { get; private set; }
        public static LuaMouseWrapper LuaMouseWrapper { get; set; }
        public static LuaEventsWrapper LuaEventsWrapper { get; private set; }

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

            LuaScript.Options.DebugPrint = LuaPrint;
            LuaScript.Globals["Profile"] = LuaProfileWrapper;
            LuaScript.Globals["Events"] = LuaEventsWrapper;
            LuaScript.Globals["Brushes"] = LuaBrushWrapper;
            LuaScript.Globals["Keyboard"] = LuaKeyboardWrapper;
            LuaScript.Globals["Mouse"] = LuaMouseWrapper;

            if (ProfileModel == null)
                return;
            if (ProfileModel.LuaScript.IsNullOrEmpty())
                return;

            try
            {
                lock (LuaEventsWrapper.InvokeLock)
                {
                    lock (LuaScript)
                    {
                        LuaScript.DoString(ProfileModel.LuaScript);
                    }
                }
            }
            catch (InternalErrorException e)
            {
                Logger.Error(e, "[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (SyntaxErrorException e)
            {
                Logger.Error(e, "[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
            catch (ScriptRuntimeException e)
            {
                Logger.Error(e, "[{0}-LUA]: Error: {1}", ProfileModel.Name, e.DecoratedMessage);
            }
        }

        #region Private lua functions

        private static void LuaPrint(string s)
        {
            Logger.Debug("[{0}-LUA]: {1}", ProfileModel?.Name, s);
        }

        #endregion

        public static void Clear()
        {
            lock (LuaScript)
            {
                // Clear old fields/properties
                KeyboardProvider = null;
                ProfileModel = null;
                LuaKeyboardWrapper?.Dispose();
                LuaKeyboardWrapper = null;

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
                    // Can be missing if the user script screwed up the globals
                }

            }
        }

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

        #endregion
    }
}