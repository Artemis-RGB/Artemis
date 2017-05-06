using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Artemis.Managers;
using Artemis.Profiles.Lua.Modules.Gui;
using Artemis.Services;
using Caliburn.Micro;
using MoonSharp.Interpreter;
using Ninject.Parameters;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaGuiModule : LuaModule
    {
        private readonly WindowService _windowService;
        private readonly List<LuaWindowViewModel> _windows;

        public LuaGuiModule(LuaManager luaManager, WindowService windowService) : base(luaManager)
        {
            _windowService = windowService;
            _windows = new List<LuaWindowViewModel>();
        }

        public override string ModuleName => "Gui";

        public LuaWindowViewModel CreateWindow(string title, double width, double height)
        {
            lock (_windows)
            {
                dynamic settings = new ExpandoObject();
                settings.Width = width;
                settings.Height = height;
                IParameter[] args =
                {
                    new ConstructorArgument("luaManager", LuaManager)
                };

                Execute.OnUIThread(() => _windows.Add(_windowService.ShowWindow<LuaWindowViewModel>("Artemis | " + title, settings, args)));
                return _windows.Last();
            }
        }

        public void AddEditorButton(string name, DynValue action)
        {
            Execute.OnUIThread(() => LuaManager.EditorButtons.Add(new EditorButton(LuaManager, name, action)));
        }

        public void RemoveEditorButton(string name)
        {
            var button = LuaManager.EditorButtons.FirstOrDefault(b => b.Text == name);
            if (button != null)
                Execute.OnUIThread(() => LuaManager.EditorButtons.Remove(button));
        }

        public override void Dispose()
        {
            foreach (var window in _windows)
                window.TryClose();

            Execute.OnUIThread(() => LuaManager.EditorButtons.Clear());
        }
    }
}
