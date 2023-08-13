using System.Net.Http.Headers;

namespace Artemis.WebClient.Workshop.Services;

public class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IAuthenticationService _authenticationService;

    /// <inheritdoc />
    public AuthenticationDelegatingHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = await _authenticationService.GetBearer();
        if (token != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}