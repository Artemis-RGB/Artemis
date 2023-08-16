using System.Reflection;
using Artemis.WebClient.Workshop.Extensions;
using Artemis.WebClient.Workshop.Repositories;
using Artemis.WebClient.Workshop.Services;
using Artemis.WebClient.Workshop.State;
using Artemis.WebClient.Workshop.UploadHandlers;
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
        Assembly[] workshopAssembly = {typeof(WorkshopConstants).Assembly};
        
        ServiceCollection serviceCollection = new();
        serviceCollection
            .AddHttpClient()
            .AddWorkshopClient()
            .AddHttpMessageHandler<WorkshopClientStoreAccessor, AuthenticationDelegatingHandler>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(WorkshopConstants.WORKSHOP_URL + "/graphql"));
        serviceCollection.AddHttpClient(WorkshopConstants.WORKSHOP_CLIENT_NAME)
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(WorkshopConstants.WORKSHOP_URL));

        serviceCollection.AddSingleton<IDiscoveryCache>(r =>
        {
            IHttpClientFactory factory = r.GetRequiredService<IHttpClientFactory>();
            return new DiscoveryCache(WorkshopConstants.AUTHORITY_URL, () => factory.CreateClient());
        });

        container.WithDependencyInjectionAdapter(serviceCollection);

        container.Register<IAuthenticationRepository, AuthenticationRepository>(Reuse.Singleton);
        container.Register<IAuthenticationService, AuthenticationService>(Reuse.Singleton);
        container.Register<IWorkshopService, WorkshopService>(Reuse.Singleton);
        
        container.Register<EntryUploadHandlerFactory>(Reuse.Transient);
        container.RegisterMany(workshopAssembly, type => type.IsAssignableTo<IEntryUploadHandler>(), Reuse.Transient);
    }
}