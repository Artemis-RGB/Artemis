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
    public class LuaWrapper : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public LuaWrapper(ProfileModel profileModel, KeyboardProvider keyboardProvider)
        {
            ProfileModel = profileModel;
            LuaProfileWrapper = new LuaProfileWrapper(ProfileModel);
            LuaBrushWrapper = new LuaBrushWrapper();
            LuaKeyboardWrapper = new LuaKeyboardWrapper(this, keyboardProvider);
            SetupLuaScript();
        }

        public ProfileModel ProfileModel { get; set; }
        public LuaProfileWrapper LuaProfileWrapper { get; set; }
        public LuaBrushWrapper LuaBrushWrapper { get; set; }
        public LuaKeyboardWrapper LuaKeyboardWrapper { get; set; }
        public LuaEventsWrapper LuaEventsWrapper { get; set; }
        public Script LuaScript { get; set; }

        private void SetupLuaScript()
        {
            LuaEventsWrapper = new LuaEventsWrapper();
            LuaScript = new Script(CoreModules.Preset_SoftSandbox);

            LuaScript.Options.DebugPrint = LuaPrint;
            LuaScript.Globals["Profile"] = LuaProfileWrapper;
            LuaScript.Globals["Events"] = LuaEventsWrapper;
            LuaScript.Globals["Brushes"] = LuaBrushWrapper;
            LuaScript.Globals["Keyboard"] = LuaKeyboardWrapper;

            if (ProfileModel.LuaScript.IsNullOrEmpty())
                return;

            try
            {
                LuaScript.DoString(ProfileModel.LuaScript);
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

        private void LuaPrint(string s)
        {
            Logger.Debug("[{0}-LUA]: {1}", ProfileModel.Name, s);
        }

        #endregion

        #region Editor

        public void OpenEditor()
        {
            // Create a temp file
            var fileName = Guid.NewGuid() + ".lua";
            var file = File.Create(Path.GetTempPath() + fileName);
            file.Dispose();

            // Add instructions to LUA script if it's a new file
            if (ProfileModel.LuaScript.IsNullOrEmpty())
                ProfileModel.LuaScript = Encoding.UTF8.GetString(Resources.lua_placeholder);
            File.WriteAllText(Path.GetTempPath() + fileName, ProfileModel.LuaScript);

            // Watch the file for changes
            var watcher = new FileSystemWatcher(Path.GetTempPath(), fileName);
            watcher.Changed += LuaFileChanged;
            watcher.EnableRaisingEvents = true;

            // Open the temp file with the default editor
            System.Diagnostics.Process.Start(Path.GetTempPath() + fileName);
        }

        private void LuaFileChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            if (fileSystemEventArgs.ChangeType != WatcherChangeTypes.Changed)
                return;

            using (var fs = new FileStream(fileSystemEventArgs.FullPath,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    ProfileModel.LuaScript = sr.ReadToEnd();
                }
            }

            ProfileProvider.AddOrUpdate(ProfileModel);
            SetupLuaScript();
        }

        #endregion

        #region Event triggers

        public void TriggerUpdate()
        {
        }

        public void TriggerDraw()
        {
        }

        #endregion

        public void Dispose()
        {
            LuaKeyboardWrapper.Dispose();
            LuaScript.Globals.Clear();
            LuaScript.Registry.Clear();
            LuaScript = null;
        }
    }
}