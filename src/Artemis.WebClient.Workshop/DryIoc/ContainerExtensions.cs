using Artemis.WebClient.Workshop.Repositories;
using Artemis.WebClient.Workshop.Services;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Artemis.WebClient.Workshop.DryIoc;

/// <summary>
/// Provides an extension method to register services onto a DryIoc <see cref="IContainer"/>.
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Registers the updating client into the container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    public static void RegisterWorkshopClient(this IContainer container)
    {
        ServiceCollection serviceCollection = new();
        serviceCollection
            .AddHttpClient()
            .AddWorkshopClient()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7281/graphql"));

        serviceCollection.AddSingleton<IDiscoveryCache>(r =>
        {
            IHttpClientFactory factory = r.GetRequiredService<IHttpClientFactory>();
            return new DiscoveryCache(IAuthenticationService.AUTHORITY, () => factory.CreateClient());
        });

        container.WithDependencyInjectionAdapter(serviceCollection);

        container.Register<IAuthenticationRepository, AuthenticationRepository>(Reuse.Singleton);
        container.Register<IAuthenticationService, AuthenticationService>(Reuse.Singleton);
    }
}