using System.Collections.ObjectModel;
using System.Security.Claims;
using Artemis.Core.Services;

namespace Artemis.WebClient.Workshop.Services;

public interface IAuthenticationService : IProtectedArtemisService
{
    IObservable<bool> IsLoggedIn { get; }
    ReadOnlyObservableCollection<Claim> Claims { get; }

    Task<string?> GetBearer();
    Task<bool> AutoLogin();
    Task Login(CancellationToken cancellationToken);
    void Logout();
}