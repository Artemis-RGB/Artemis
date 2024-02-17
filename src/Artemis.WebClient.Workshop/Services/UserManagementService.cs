using System.Net.Http.Headers;
using System.Net.Http.Json;
using Artemis.WebClient.Workshop.Exceptions;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Models;

namespace Artemis.WebClient.Workshop.Services;

internal class UserManagementService : IUserManagementService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UserManagementService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<ApiResult> ChangePassword(string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.IDENTITY_CLIENT_NAME);
        HttpResponseMessage response = await client.PostAsync("user/credentials", JsonContent.Create(new {CurrentPassword = currentPassword, NewPassword = newPassword}), cancellationToken);
        return response.IsSuccessStatusCode 
            ? ApiResult.FromSuccess() 
            : ApiResult.FromFailure(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiResult> ChangeEmailAddress(string emailAddress, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.IDENTITY_CLIENT_NAME);
        HttpResponseMessage response = await client.PostAsync("user/email", JsonContent.Create(new {EmailAddress = emailAddress}), cancellationToken);
        return response.IsSuccessStatusCode 
            ? ApiResult.FromSuccess() 
            : ApiResult.FromFailure(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<ApiResult> ChangeAvatar(Stream avatar, CancellationToken cancellationToken)
    {
        avatar.Seek(0, SeekOrigin.Begin);

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.IDENTITY_CLIENT_NAME);

        // Construct the request
        MultipartFormDataContent content = new();
        StreamContent streamContent = new(avatar);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(streamContent, "file", "file.png");

        // Submit
        HttpResponseMessage response = await client.PutAsync($"user/avatar", content, cancellationToken);
        return response.IsSuccessStatusCode 
            ? ApiResult.FromSuccess() 
            : ApiResult.FromFailure(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ApiResult> RemoveAccount(CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.IDENTITY_CLIENT_NAME);
        HttpResponseMessage response = await client.DeleteAsync("user", cancellationToken);
        return response.IsSuccessStatusCode 
            ? ApiResult.FromSuccess() 
            : ApiResult.FromFailure(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <inheritdoc />
    public async Task<string> CreatePersonAccessToken(string description, DateTime expirationDate, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.IDENTITY_CLIENT_NAME);
        HttpResponseMessage response = await client.PostAsync("user/access-token", JsonContent.Create(new {Description = description, ExpirationDate = expirationDate}), cancellationToken);
        response.EnsureSuccessStatusCode();
        
        string? result = await response.Content.ReadAsStringAsync(cancellationToken);
        if (result == null)
            throw new ArtemisWebClientException("Failed to deserialize access token");
        return result;
    }

    /// <inheritdoc />
    public async Task<ApiResult> DeletePersonAccessToken(PersonalAccessToken personalAccessToken, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.IDENTITY_CLIENT_NAME);
        HttpResponseMessage response = await client.DeleteAsync($"user/access-token/{personalAccessToken.Key}", cancellationToken);
        return response.IsSuccessStatusCode 
            ? ApiResult.FromSuccess() 
            : ApiResult.FromFailure(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    /// <inheritdoc />
    public async Task<List<PersonalAccessToken>> GetPersonAccessTokens(CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.IDENTITY_CLIENT_NAME);
        HttpResponseMessage response = await client.GetAsync("user/access-token", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        List<PersonalAccessToken>? result = await response.Content.ReadFromJsonAsync<List<PersonalAccessToken>>(cancellationToken: cancellationToken);
        if (result == null)
            throw new ArtemisWebClientException("Failed to deserialize access tokens");
        return result;
    }
}