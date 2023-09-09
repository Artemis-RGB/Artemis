using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using StrawberryShake;

namespace Artemis.WebClient.Workshop.Extensions;

public static class ClientBuilderExtensions
{
    public static IClientBuilder<T> AddHttpMessageHandler<T, THandler>(this IClientBuilder<T> builder) where THandler : DelegatingHandler where T : IStoreAccessor
    {
        builder.Services.Configure<HttpClientFactoryOptions>(
            builder.ClientName,
            options => options.HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Add(b.Services.GetRequiredService<THandler>()))
        );

        return builder;
    }
}