using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Claims;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.WebClient.Workshop.Repositories;
using IdentityModel;
using IdentityModel.Client;

namespace Artemis.WebClient.Workshop.Services;

public interface IAuthenticationService : IProtectedArtemisService
{
    public const string AUTHORITY = "https://localhost:5001";
    
    bool IsLoggedIn { get; }
    string? UserCode { get; }
    ReadOnlyObservableCollection<Claim> Claims { get; }

    Task<string?> GetBearer();
    Task<bool> AutoLogin();
    Task<bool> Login();
    void Logout();
}

internal class AuthenticationService : CorePropertyChanged, IAuthenticationService
{
    internal const string CLIENT_ID = "artemis.desktop";

    private readonly IDiscoveryCache _discoveryCache;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ObservableCollection<Claim> _claims = new();
    private readonly SemaphoreSlim _bearerLock = new(1);

    private AuthenticationToken? _token;
    private string? _userCode;
    private bool _isLoggedIn;

    public AuthenticationService(IHttpClientFactory httpClientFactory, IDiscoveryCache discoveryCache, IAuthenticationRepository authenticationRepository)
    {
        _httpClientFactory = httpClientFactory;
        _discoveryCache = discoveryCache;
        _authenticationRepository = authenticationRepository;

        Claims = new ReadOnlyObservableCollection<Claim>(_claims);
    }

    /// <inheritdoc />
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        private set => SetAndNotify(ref _isLoggedIn, value);
    }

    /// <inheritdoc />
    public string? UserCode
    {
        get => _userCode;
        private set => SetAndNotify(ref _userCode, value);
    }

    /// <inheritdoc />
    public ReadOnlyObservableCollection<Claim> Claims { get; }

    public async Task<string?> GetBearer()
    {
        await _bearerLock.WaitAsync();

        try
        {
            // If not logged in, attempt to auto login first
            if (!IsLoggedIn)
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
        if (IsLoggedIn)
            return true;
        
        string? refreshToken = _authenticationRepository.GetRefreshToken();
        if (refreshToken == null)
            return false;

        return await UseRefreshToken(refreshToken);
    }

    /// <inheritdoc />
    public async Task<bool> Login()
    {
        DiscoveryDocumentResponse disco = await GetDiscovery();
        HttpClient client = _httpClientFactory.CreateClient();
        DeviceAuthorizationResponse response = await client.RequestDeviceAuthorizationAsync(new DeviceAuthorizationRequest
        {
            Address = disco.DeviceAuthorizationEndpoint,
            ClientId = CLIENT_ID
        });
        if (response.IsError)
            throw new ArtemisWebClientException("Failed to request device authorization: " + response.Error);
        if (response.DeviceCode == null)
            throw new ArtemisWebClientException("Failed to request device authorization: Got no device code");

        DateTimeOffset timeout = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn ?? 1800);

        Process.Start(new ProcessStartInfo {FileName = response.VerificationUriComplete, UseShellExecute = true});
        await Task.Delay(TimeSpan.FromSeconds(response.Interval));
        while (DateTimeOffset.UtcNow < timeout)
        {
            bool success = await AttemptRequestDeviceToken(client, response.DeviceCode);
            if (success)
                return true;
            await Task.Delay(TimeSpan.FromSeconds(response.Interval));
        }

        return false;
    }

    /// <inheritdoc />
    public void Logout()
    {
        _token = null;
        _claims.Clear();
        _authenticationRepository.SetRefreshToken(null);
        
        IsLoggedIn = false;
    }

    private async Task<DiscoveryDocumentResponse> GetDiscovery()
    {
        DiscoveryDocumentResponse disco = await _discoveryCache.GetAsync();
        if (disco.IsError)
            throw new ArtemisWebClientException("Failed to retrieve discovery document: " + disco.Error);

        return disco;
    }

    private async Task<bool> AttemptRequestDeviceToken(HttpClient client, string deviceCode)
    {
        DiscoveryDocumentResponse disco = await GetDiscovery();
        TokenResponse response = await client.RequestDeviceTokenAsync(new DeviceTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = CLIENT_ID,
            DeviceCode = deviceCode
        });

        if (response.IsError)
        {
            if (response.Error == OidcConstants.TokenErrors.AuthorizationPending || response.Error == OidcConstants.TokenErrors.SlowDown)
                return false;

            throw new ArtemisWebClientException("Failed to request device token: " + response.Error);
        }

        await SetCurrentUser(client, response);
        return true;
    }

    private async Task SetCurrentUser(HttpClient client, TokenResponse response)
    {
        _token = new AuthenticationToken(response);
        _authenticationRepository.SetRefreshToken(_token.RefreshToken);

        await GetUserInfo(client, _token.AccessToken);
        IsLoggedIn = true;
    }

    private async Task GetUserInfo(HttpClient client, string accessToken)
    {
        DiscoveryDocumentResponse disco = await GetDiscovery();
        UserInfoResponse response = await client.GetUserInfoAsync(new UserInfoRequest()
        {
            Address = disco.UserInfoEndpoint,
            Token = accessToken
        });
        if (response.IsError)
            throw new ArtemisWebClientException("Failed to retrieve user info: " + response.Error);

        _claims.Clear();
        foreach (Claim responseClaim in response.Claims)
            _claims.Add(responseClaim);
    }

    private async Task<bool> UseRefreshToken(string refreshToken)
    {
        DiscoveryDocumentResponse disco = await GetDiscovery();
        HttpClient client = _httpClientFactory.CreateClient();
        TokenResponse response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest()
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

        await SetCurrentUser(client, response);
        return false;
    }
}