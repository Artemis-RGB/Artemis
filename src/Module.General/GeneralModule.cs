using System;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;

namespace Module.General
{
    public class GeneralModule : IModule
    {
        public GeneralModule(ICoreService coreService)
        {
            Console.WriteLine(coreService);
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