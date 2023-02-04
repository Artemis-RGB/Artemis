using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Artemis.WebClient.Updating.DryIoc;

/// <summary>
/// Provides an extension method to register services onto a DryIoc <see cref="IContainer"/>.
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Registers the updating client into the container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    public static void RegisterUpdatingClient(this IContainer container)
    {
        ServiceCollection serviceCollection = new();
        serviceCollection
            .AddHttpClient()
            .AddUpdatingClient()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://updating.artemis-rgb.com/graphql"));
        
        container.WithDependencyInjectionAdapter(serviceCollection);
    }
}