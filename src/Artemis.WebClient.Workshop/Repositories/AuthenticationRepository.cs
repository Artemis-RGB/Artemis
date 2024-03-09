using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.WebClient.Workshop.Repositories;

internal class AuthenticationRepository : IAuthenticationRepository
{
    private readonly PluginSetting<string> _refreshToken;

    public AuthenticationRepository(ISettingsService settingsService)
    {
        // Of course anyone can grab these indirectly, but that goes for whatever we do.
        // ISettingsService is a protected service so we at least don't make it very straightforward.
        _refreshToken = settingsService.GetSetting<string>("Workshop.RefreshToken");
    }

    /// <inheritdoc />
    public void SetRefreshToken(string? refreshToken)
    {
        _refreshToken.Value = refreshToken;
        _refreshToken.Save();
    }

    /// <inheritdoc />
    public string? GetRefreshToken()
    {
        return _refreshToken.Value;
    }
}

internal interface IAuthenticationRepository
{
    void SetRefreshToken(string? refreshToken);
    string? GetRefreshToken();
}