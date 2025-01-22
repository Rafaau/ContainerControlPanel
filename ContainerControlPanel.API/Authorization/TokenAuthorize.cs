using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ContainerControlPanel.API.Authorization;

/// <summary>
/// Attribute for authorizing a request with a token.
/// </summary>
public class TokenAuthorize : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenAuthorize"/> class.
    /// </summary>
    public TokenAuthorize() : base(typeof(SessionRequirementFilter))
    {
    }
}

/// <summary>
/// Filter for session requirements.
/// </summary>
public class SessionRequirementFilter : IAuthorizationFilter
{
    /// <summary>
    /// Session validation service.
    /// </summary>
    private readonly SessionValidation _sessionValidation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionRequirementFilter"/> class.
    /// </summary>
    /// <param name="sessionValidation">Session validation service</param>
    public SessionRequirementFilter(SessionValidation sessionValidation)
    {
        _sessionValidation = sessionValidation;
    }

    /// <summary>
    /// Authorizes the request.
    /// </summary>
    /// <param name="context">Authorization filter context</param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!_sessionValidation.ContainsToken(context))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}

/// <summary>
/// Session validation service.
/// </summary>
public class SessionValidation 
{
    /// <summary>
    /// Configuration settings.
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionValidation"/> class.
    /// </summary>
    /// <param name="configuration">Configuration settings</param>
    public SessionValidation(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Checks if the request contains a valid token.
    /// </summary>
    /// <param name="context">Authorization filter context</param>
    /// <returns>Returns true if the request contains a valid token</returns>
    public bool ContainsToken(AuthorizationFilterContext context)
        => context.HttpContext.Request.Headers.ContainsKey("Authorization")
            && context.HttpContext.Request.Headers["Authorization"].ToString().StartsWith(_configuration["AuthToken"]);
}
