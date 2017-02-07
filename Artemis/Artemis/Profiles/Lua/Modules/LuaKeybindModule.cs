using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Artemis.Managers;
using Artemis.Models;
using MahApps.Metro.Controls;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaKeybindModule : LuaModule
    {
        private readonly List<KeybindModel> _keybindModels;

        public LuaKeybindModule(LuaManager luaManager) : base(luaManager)
        {
            _keybindModels = new List<KeybindModel>();
            LuaManager.ProfileModel.OnProfileUpdatedEvent += ProfileModelOnOnProfileUpdatedEvent;
        }

        public override string ModuleName => "Keybind";

        private void ProfileModelOnOnProfileUpdatedEvent(object sender, EventArgs e)
        {
            foreach (var keybindModel in _keybindModels)
                KeybindManager.AddOrUpdate(keybindModel);
        }

        /// <summary>
        ///     Sets a keybind to call the provided function
        /// </summary>
        /// <param name="name">Name of the keybind</param>
        /// <param name="hotKey">Hotkey in string format, per example: ALT+CTRL+SHIFT+D</param>
        /// <param name="function">LUA function to call</param>
        /// <param name="args">Optional arguments for the passed function</param>
        public void SetKeybind(string name, string hotKey, DynValue function, params DynValue[] args)
        {
            var modifierKeys = ModifierKeys.None;
            var key = Key.System;
            var hotKeyParts = hotKey.Split('+').Select(p => p.Trim());
            foreach (var hotKeyPart in hotKeyParts)
                if (hotKeyPart == "ALT")
                    modifierKeys |= ModifierKeys.Alt;
                else if (hotKeyPart == "CTRL")
                    modifierKeys |= ModifierKeys.Control;
                else if (hotKeyPart == "SHIFT")
                    modifierKeys |= ModifierKeys.Shift;
                else
                    Enum.TryParse(hotKeyPart, true, out key);

            if (key == Key.System)
                throw new ScriptRuntimeException($"Hotkey '{hotKey}' couldn't be parsed.");

            var hk = new HotKey(key, modifierKeys);
            var model = args != null
                ? new KeybindModel("LUA-" + name, hk, () => LuaManager.Call(function, args))
                : new KeybindModel("LUA-" + name, hk, () => LuaManager.Call(function));

            KeybindManager.AddOrUpdate(model);

            var existing = _keybindModels.FirstOrDefault(k => k.Name == model.Name);
            if (existing != null)
                _keybindModels.Remove(existing);

            _keybindModels.Add(model);
        }

        /// <summary>
        ///     If found, removes a keybind with the given name
        /// </summary>
        /// <param name="name"></param>
        public void RemoveKeybind(string name)
        {
            var existing = _keybindModels.FirstOrDefault(k => k.Name == name);
            if (existing != null)
                _keybindModels.Remove(existing);

            KeybindManager.Remove(name);
        }

        public override void Dispose()
        {
            foreach (var keybindModel in _keybindModels)
                KeybindManager.Remove(keybindModel);

            LuaManager.ProfileModel.OnProfileUpdatedEvent -= ProfileModelOnOnProfileUpdatedEvent;
        }
    }
}