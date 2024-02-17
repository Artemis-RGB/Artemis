using Artemis.Core.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Models;

namespace Artemis.WebClient.Workshop.Services;

public interface IUserManagementService : IProtectedArtemisService
{
    Task<ApiResult> ChangePassword(string currentPassword, string newPassword, CancellationToken cancellationToken);
    Task<ApiResult> ChangeEmailAddress(string emailAddress, CancellationToken cancellationToken);
    Task<ApiResult> ChangeAvatar(Stream avatar, CancellationToken cancellationToken);
    Task<ApiResult> RemoveAccount(CancellationToken cancellationToken);
    Task<string> CreatePersonAccessToken(string description, DateTime expirationDate, CancellationToken cancellationToken);
    Task<ApiResult> DeletePersonAccessToken(PersonalAccessToken personalAccessToken, CancellationToken cancellationToken);
    Task<List<PersonalAccessToken>> GetPersonAccessTokens(CancellationToken cancellationToken);
}