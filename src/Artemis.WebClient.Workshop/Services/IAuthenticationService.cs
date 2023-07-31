using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Claims;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.WebClient.Workshop.Repositories;
using IdentityModel;
using IdentityModel.Client;

namespace Artemis.WebClient.Workshop.Services;

public interface IAuthenticationService : IProtectedArtemisService
{
    IObservable<bool> IsLoggedIn { get; }
    IObservable<string?> UserCode { get; }
    IObservable<string?> VerificationUri { get; }
    ReadOnlyObservableCollection<Claim> Claims { get; }

    Task<string?> GetBearer();
    Task<bool> AutoLogin();
    Task<bool> Login(CancellationToken cancellationToken);
    void Logout();
}

internal class AuthenticationService : CorePropertyChanged, IAuthenticationService
{
    private const string CLIENT_ID = "artemis.desktop";
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly SemaphoreSlim _bearerLock = new(1);
    private readonly ObservableCollection<Claim> _claims = new();

    private readonly IDiscoveryCache _discoveryCache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BehaviorSubject<bool> _isLoggedInSubject = new(false);
    private readonly BehaviorSubject<string?> _userCodeSubject = new(null);
    private readonly BehaviorSubject<string?> _verificationUriSubject = new(null);

    private AuthenticationToken? _token;

    public AuthenticationService(IHttpClientFactory httpClientFactory, IDiscoveryCache discoveryCache, IAuthenticationRepository authenticationRepository)
    {
        _httpClientFactory = httpClientFactory;
        _discoveryCache = discoveryCache;
        _authenticationRepository = authenticationRepository;

        Claims = new ReadOnlyObservableCollection<Claim>(_claims);
    }

    private async Task<DiscoveryDocumentResponse> GetDiscovery()
    {
        DiscoveryDocumentResponse disco = await _discoveryCache.GetAsync();
        if (disco.IsError)
            throw new ArtemisWebClientException("Failed to retrieve discovery document: " + disco.Error);

        return disco;
    }

    private async Task<bool> AttemptRequestDeviceToken(HttpClient client, string deviceCode, CancellationToken cancellationToken)
    {
        DiscoveryDocumentResponse disco = await GetDiscovery();
        TokenResponse response = await client.RequestDeviceTokenAsync(new DeviceTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = CLIENT_ID,
            DeviceCode = deviceCode
        }, cancellationToken);

        if (response.IsError)
        {
            if (response.Error == OidcConstants.TokenErrors.AuthorizationPending || response.Error == OidcConstants.TokenErrors.SlowDown)
                return false;

            throw new ArtemisWebClientException("Failed to request device token: " + response.Error);
        }

        SetCurrentUser(response);
        return true;
    }

    private void SetCurrentUser(TokenResponse response)
    {
        _token = new AuthenticationToken(response);
        _authenticationRepository.SetRefreshToken(_token.RefreshToken);

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? token = handler.ReadJwtToken(response.IdentityToken);
        if (token == null)
            throw new ArtemisWebClientException("Failed to read JWT token");

        _claims.Clear();
        foreach (Claim responseClaim in token.Claims)
            _claims.Add(responseClaim);

        _isLoggedInSubject.OnNext(true);
    }

    private async Task<bool> UseRefreshToken(string refreshToken)
    {
        DiscoveryDocumentResponse disco = await GetDiscovery();
        HttpClient client = _httpClientFactory.CreateClient();
        TokenResponse response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = CLIENT_ID,
            RefreshToken = refreshToken
        });

        if (response.IsError)
        {
            if (response.Error == OidcConstants.TokenErrors.ExpiredToken)
                return false;

            throw new ArtemisWebClientException("Failed to request refresh token: " + response.Error);
        }

        SetCurrentUser(response);
        return false;
    }

    /// <inheritdoc />
    public IObservable<bool> IsLoggedIn => _isLoggedInSubject.AsObservable();

    /// <inheritdoc />
    public IObservable<string?> UserCode => _userCodeSubject.AsObservable();

    /// <inheritdoc />
    public IObservable<string?> VerificationUri => _verificationUriSubject.AsObservable();

    /// <inheritdoc />
    public ReadOnlyObservableCollection<Claim> Claims { get; }

    public async Task<string?> GetBearer()
    {
        await _bearerLock.WaitAsync();

        try
        {
            // If not logged in, attempt to auto login first
            if (!_isLoggedInSubject.Value)
                await AutoLogin();

            if (_token == null)
                return null;

            // If the token is expiring, refresh it
            if (_token.Expired && !await UseRefreshToken(_token.RefreshToken))
                return null;

            return _token.AccessToken;
        }
        finally
        {
            _bearerLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> AutoLogin()
    {
        if (_isLoggedInSubject.Value)
            return true;

        string? refreshToken = _authenticationRepository.GetRefreshToken();
        if (refreshToken == null)
            return false;

        return await UseRefreshToken(refreshToken);
    }

    /// <inheritdoc />
    public async Task<bool> Login(CancellationToken cancellationToken)
    {
        DiscoveryDocumentResponse disco = await GetDiscovery();
        HttpClient client = _httpClientFactory.CreateClient();
        DeviceAuthorizationResponse response = await client.RequestDeviceAuthorizationAsync(new DeviceAuthorizationRequest
        {
            Address = disco.DeviceAuthorizationEndpoint,
            ClientId = CLIENT_ID,
            Scope = "openid profile email offline_access api"
        }, cancellationToken);
        if (response.IsError)
            throw new ArtemisWebClientException("Failed to request device authorization: " + response.Error);
        if (response.DeviceCode == null)
            throw new ArtemisWebClientException("Failed to request device authorization: Got no device code");

        DateTimeOffset timeout = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn ?? 1800);
        _userCodeSubject.OnNext(response.UserCode);
        _verificationUriSubject.OnNext(response.VerificationUri);

        await Task.Delay(TimeSpan.FromSeconds(response.Interval), cancellationToken);
        try
        {
            while (DateTimeOffset.UtcNow < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool success = await AttemptRequestDeviceToken(client, response.DeviceCode, cancellationToken);
                if (success)
                    return true;
                await Task.Delay(TimeSpan.FromSeconds(response.Interval), cancellationToken);
            }

            return false;
        }
        finally
        {
            _userCodeSubject.OnNext(null);
            _verificationUriSubject.OnNext(null);
        }
    }

    /// <inheritdoc />
    public void Logout()
    {
        _token = null;
        _claims.Clear();
        _authenticationRepository.SetRefreshToken(null);

        _isLoggedInSubject.OnNext(false);
    }
}