using Blazored.SessionStorage;
using ContainerControlPanel.Web.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ContainerControlPanel.Web.Authentication;

/// <summary>
/// Custom authentication state provider.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    /// <summary>
    /// Session storage service instance.
    /// </summary>
    private readonly ISessionStorageService _sessionStorage;

    /// <summary>
    /// Anonymous user principal.
    /// </summary>
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    /// <summary>
    /// Constructor of the <see cref="CustomAuthenticationStateProvider"/> class.
    /// </summary>
    /// <param name="sessionStorage">Session storage service instance.</param>
    public CustomAuthenticationStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    /// <summary>
    /// Get the current authentication state.
    /// </summary>
    /// <returns>Returns the current authentication state.</returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserSession>("UserSession");

            if (userSession == null)
                return new AuthenticationState(_anonymous);

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, userSession.UserName),
                new Claim(ClaimTypes.Role, userSession.Role),
            }, "session"));

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    /// <summary>
    /// Updates the authentication state.
    /// </summary>
    /// <param name="userSession">User session object.</param>
    /// <returns>Returns the task object.</returns>
    public async Task UpdateAuthenticationState(UserSession? userSession)
    {
        ClaimsPrincipal claimsPrincipal;

        if (userSession != null)
        {
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, userSession.UserName),
                new Claim(ClaimTypes.Role, userSession.Role),
            }, "session"));
            userSession.ExpiryTimeStamp = DateTime.Now.AddSeconds(userSession.ExpiresIn);
            await _sessionStorage.SaveItemEncryptedAsync("UserSession", userSession);
        }
        else
        {
            claimsPrincipal = _anonymous;
            await _sessionStorage.RemoveItemAsync("UserSession");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    /// <summary>
    /// Gets the token.
    /// </summary>
    /// <returns>Returns the token.</returns>
    public async Task<string> GetToken()
    {
        var result = string.Empty;

        try
        {
            var userSession = await _sessionStorage.ReadEncryptedItemAsync<UserSession>("UserSession");
            if (userSession != null && DateTime.Now < userSession.ExpiryTimeStamp)
                result = userSession.Token;
        }
        catch { }

        return result;
    }
}
