using System;
using Artemis.UI.Shared.Services.Interfaces;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.UI.Shared.Ninject
{
    /// <summary>
    ///     The main <see cref="NinjectModule" /> of the Artemis Shared UI toolkit that binds all services
    /// </summary>
    public class SharedUIModule : NinjectModule
    {
        /// <inheritdoc />
        public override void Load()
        {
            if (Kernel == null)
                throw new ArgumentNullException("Kernel shouldn't be null here.");

            // Bind all shared UI services as singletons
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IArtemisSharedUIService>()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisSharedUIService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });
        }
    }
}