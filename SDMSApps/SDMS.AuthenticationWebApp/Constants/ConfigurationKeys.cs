namespace SDMS.AuthenticationWebApp.Constants;

/// <summary>
/// Configuration key constants for appsettings.json
/// Uses SDMS_AuthenticationWebApp_ prefix for consistency with other SDMS apps
/// Environment variables should use the same keys (e.g., SDMS_AuthenticationWebApp_ConnectionString)
/// </summary>
public static class ConfigurationKeys
{
    // Connection String
    // Priority: SDMS_AuthenticationWebApp_ConnectionString > POSTGRES_CONNECTION > Default
    public const string ConnectionString = "SDMS_AuthenticationWebApp_ConnectionString";
    public const string PostgresConnection = "POSTGRES_CONNECTION"; // Railway automatic env var (fallback)
    
    // Frontend Configuration
    public const string FrontendUrl = "SDMS_AuthenticationWebApp_FrontendUrl";
    
    // Authentication URLs
    public const string LoginUrl = "SDMS_AuthenticationWebApp_LoginUrl";
    public const string LogoutUrl = "SDMS_AuthenticationWebApp_LogoutUrl";
    public const string ErrorUrl = "SDMS_AuthenticationWebApp_ErrorUrl";
    public const string ReturnUrlParameter = "SDMS_AuthenticationWebApp_ReturnUrlParameter";
    
    // Server Configuration
    public const string ServerPort = "SDMS_AuthenticationWebApp_ServerPort";
    public const string ServerUrls = "SDMS_AuthenticationWebApp_ServerUrls";
    
    // External Authentication (Google)
    public const string ExternalAuthGoogleClientId = "SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId";
    public const string ExternalAuthGoogleClientSecret = "SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientSecret";
    
    // External Authentication (Auth0)
    public const string ExternalAuthAuth0Domain = "SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain";
    public const string ExternalAuthAuth0ClientId = "SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId";
    public const string ExternalAuthAuth0ClientSecret = "SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientSecret";
    
    // External Authentication (Redirect URI)
    public const string ExternalAuthRedirectUri = "SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri";
    
    // Webhook
    public const string WebhookSecret = "SDMS_AuthenticationWebApp_WebhookSecret";
    
    // Signing Key
    public const string SigningKeyPath = "SDMS_AuthenticationWebApp_SigningKeyPath";
    
    // OpenIddict Client Configuration
    public const string RedirectUris = "SDMS_AuthenticationWebApp_RedirectUris";
    public const string PostLogoutRedirectUris = "SDMS_AuthenticationWebApp_PostLogoutRedirectUris";
}

