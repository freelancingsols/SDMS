namespace SDMS.AuthenticationWebApp.Constants;

/// <summary>
/// Configuration key constants for appsettings.json
/// </summary>
public static class ConfigurationKeys
{
    public const string AuthenticationLoginUrl = "Authentication:LoginUrl";
    public const string AuthenticationLogoutUrl = "Authentication:LogoutUrl";
    public const string AuthenticationErrorUrl = "Authentication:ErrorUrl";
    public const string AuthenticationReturnUrlParameter = "Authentication:ReturnUrlParameter";
    
    public const string PostgresConnection = "POSTGRES_CONNECTION";
    public const string DefaultConnection = "ConnectionStrings:DefaultConnection";
    
    public const string FrontendUrl = "Frontend:Url";
    
    public const string ExternalAuthGoogleClientId = "ExternalAuth:Google:ClientId";
    public const string ExternalAuthGoogleClientSecret = "ExternalAuth:Google:ClientSecret";
    
    public const string WebhookSecret = "Webhook:Secret";
}

