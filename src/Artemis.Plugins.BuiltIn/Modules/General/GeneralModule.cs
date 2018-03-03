using System.Diagnostics;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;

namespace Artemis.Plugins.BuiltIn.Modules.General
{
    public class GeneralModule : IModule
    {
        private readonly ICoreService _coreService;

        public GeneralModule(ICoreService coreService)
        {
            _coreService = coreService;

            Debugger.Break();
        }

        public void LoadPlugin()
        {
        }

        public void UnloadPlugin()
        {
        }

        public void Update(double deltaTime)
        {
        }

        public void Render(double deltaTime)
        {
        }
    }
}