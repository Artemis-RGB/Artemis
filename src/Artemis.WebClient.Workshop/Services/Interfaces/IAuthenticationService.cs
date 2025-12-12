using System.Collections.ObjectModel;
using System.Security.Claims;
using Artemis.Core.Services;

namespace Artemis.WebClient.Workshop.Services;

public interface IAuthenticationService : IProtectedArtemisService
{
    IObservable<bool> IsLoggedIn { get; }
    ReadOnlyObservableCollection<Claim> Claims { get; }

    IObservable<Claim?> GetClaim(string type);
    Task<string?> GetBearer();
    Task<bool> AutoLogin(bool force = false);
    Task Login(CancellationToken cancellationToken);
    Task Logout();
    bool GetIsEmailVerified();
    List<string> GetRoles();
}