using Artemis.Core.DryIoc;
using DryIoc;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Migrator;

class Program
{
    static void Main(string[] args)
    {
        Container container = new Container(rules => rules
            .WithMicrosoftDependencyInjectionRules()
            .WithConcreteTypeDynamicRegistrations()
            .WithoutThrowOnRegisteringDisposableTransient());

        container.RegisterCore();
        container.Resolve<ArtemisDbContext>().Database.EnsureCreated();
    }
}