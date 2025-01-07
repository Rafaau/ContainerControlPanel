namespace ContainerControlPanel.Web.Authentication;

/// <summary>
/// User session class.
/// </summary>
public class UserSession
{
    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// Gets or sets the expiry time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the expiry time stamp.
    /// </summary>
    public DateTime ExpiryTimeStamp { get; set; }
}
