using SDMS.AuthenticationWebApp.Constants;

namespace SDMS.AuthenticationWebApp.Configuration;

/// <summary>
/// Handles deployment platform-specific configuration
/// Supports multiple platforms: Railway, Azure, AWS, Local Development, etc.
/// </summary>
public static class DeploymentConfiguration
{
    /// <summary>
    /// Gets the database connection string from various sources
    /// Priority: Custom Connection String > Platform-specific URLs > Default
    /// </summary>
    public static string GetDatabaseConnectionString(IConfiguration configuration, ILogger? logger = null)
    {
        // Priority order:
        // 1. Custom application connection string (SDMS_AuthenticationWebApp_ConnectionString)
        // 2. Railway DATABASE_URL (standard for Railway deployments)
        // 3. Railway POSTGRES_URL (alternative)
        // 4. Platform-agnostic POSTGRES_CONNECTION
        // 5. Default local development connection string
        
        // Helper method to get non-empty configuration value
        string? GetConfigValue(string key) 
        {
            var value = configuration[key];
            var isNullOrEmpty = string.IsNullOrWhiteSpace(value);
            
            // Log for debugging (only in development or if logger is available)
            if (logger != null && !isNullOrEmpty)
            {
                logger.LogDebug("Found configuration value for key: {Key}", key);
            }
            else if (logger != null && isNullOrEmpty && !string.IsNullOrEmpty(key))
            {
                logger.LogDebug("Configuration key {Key} is not set or is empty", key);
            }
            
            return isNullOrEmpty ? null : value;
        }
        
        // Determine if we're in a production/deployed environment
        // Check for common cloud platform indicators
        var isProduction = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"))
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_PROJECT_ID"))
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_ENVIRONMENT"))
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_REGION"))
            || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DYNO")) // Heroku
            || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
            || (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development" 
                && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT"))); // Railway/cloud platforms set PORT
        
        // Try to get connection string from various sources
        var connectionString = GetConfigValue(ConfigurationKeys.ConnectionString)
            ?? GetConfigValue("DATABASE_URL")  // Railway standard (auto-injected when PostgreSQL service is attached)
            ?? GetConfigValue("POSTGRES_URL")  // Railway alternative
            ?? GetConfigValue("POSTGRES_CONNECTION")  // Platform-agnostic
            ?? GetConfigValue(ConfigurationKeys.PostgresConnection);
        
        // Use default only for local development
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (isProduction)
            {
                // In production, require a database connection string
                var errorMessage = "Database connection string is required in production environment. " +
                    "Please set one of the following environment variables: " +
                    "SDMS_AuthenticationWebApp_ConnectionString, DATABASE_URL, POSTGRES_URL, or POSTGRES_CONNECTION. " +
                    "If using Railway, ensure a PostgreSQL service is attached to your project.";
                
                if (logger != null)
                {
                    logger.LogError(errorMessage);
                }
                else
                {
                    Console.WriteLine($"Error: {errorMessage}");
                }
                
                throw new InvalidOperationException(errorMessage);
            }
            else
            {
                // Local development: use default
                connectionString = "Host=localhost;Database=sdms_auth;Username=postgres;Password=postgres";
                
                if (logger != null)
                {
                    logger.LogWarning("Using default localhost database connection string for local development");
                }
                else
                {
                    Console.WriteLine("Warning: Using default localhost database connection string for local development");
                }
            }
        }
        else
        {
            // Log which source was used
            if (logger != null)
            {
                logger.LogInformation("Database connection string found in environment/configuration");
            }
            else
            {
                Console.WriteLine("Database connection string found in environment/configuration");
            }
        }

        // Convert URL format (postgresql://user:pass@host:port/db) to Npgsql format if needed
        connectionString = NormalizeConnectionString(connectionString, logger);

        // Validate connection string
        ValidateConnectionString(connectionString);

        // Log connection info (without exposing password)
        LogConnectionInfo(connectionString, logger);

        return connectionString;
    }

    /// <summary>
    /// Normalizes connection string from URL format to Npgsql format
    /// Supports: postgresql://, postgres://, and standard Npgsql format
    /// </summary>
    private static string NormalizeConnectionString(string connectionString, ILogger? logger)
    {
        // If already in Npgsql format (contains Host=), return as-is
        if (connectionString.Contains("Host="))
        {
            return connectionString;
        }

        // Convert URL format to Npgsql format
        if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
        {
            try
            {
                var uri = new Uri(connectionString);
                var dbHost = uri.Host;
                var dbPort = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.LocalPath.TrimStart('/');
                var username = uri.UserInfo.Split(':')[0];
                var password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : "";

                var normalized = $"Host={dbHost};Port={dbPort};Database={database};Username={username};Password={password}";
                
                var logMessage = $"Converted database URL to Npgsql format. Host: {dbHost}, Database: {database}";
                if (logger != null)
                {
                    logger.LogInformation(logMessage);
                }
                else
                {
                    Console.WriteLine(logMessage);
                }

                return normalized;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error parsing database URL format: {ex.Message}";
                if (logger != null)
                {
                    logger.LogError(ex, "Error parsing database URL format");
                }
                else
                {
                    Console.WriteLine($"Error: {errorMessage}");
                }
                throw new InvalidOperationException(
                    "Failed to parse database connection URL. Please provide a valid PostgreSQL connection string.", ex);
            }
        }

        return connectionString;
    }

    /// <summary>
    /// Validates that the connection string is valid and contains required components
    /// </summary>
    private static void ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is required. " +
                "Set SDMS_AuthenticationWebApp_ConnectionString, DATABASE_URL, or POSTGRES_URL environment variable.");
        }

        // Validate that connection string contains Host
        var hasHost = connectionString.Contains("Host=") ||
                      connectionString.Contains("postgresql://") ||
                      connectionString.Contains("postgres://");

        if (!hasHost)
        {
            var preview = connectionString.Length > 50
                ? connectionString.Substring(0, 50) + "..."
                : connectionString;

            throw new InvalidOperationException(
                $"Database connection string is missing Host. " +
                $"Current connection string: {preview}. " +
                $"Please provide a valid PostgreSQL connection string with Host specified.");
        }
    }

    /// <summary>
    /// Logs connection information without exposing sensitive data
    /// </summary>
    private static void LogConnectionInfo(string connectionString, ILogger? logger)
    {
        try
        {
            string logInfo;
            
            if (connectionString.Contains("Host="))
            {
                // Extract Host, Database, and Port from Npgsql format
                var parts = connectionString.Split(';')
                    .Where(s => s.StartsWith("Host=") || s.StartsWith("Database=") || s.StartsWith("Port="))
                    .ToArray();
                
                logInfo = string.Join("; ", parts);
            }
            else if (connectionString.Contains("@"))
            {
                // Extract user and host from URL format
                var userPart = connectionString.Split('@')[0];
                var hostPart = connectionString.Contains("@")
                    ? connectionString.Split('@')[1].Split('/')[0]
                    : "host/database";
                logInfo = $"{userPart}@{hostPart}";
            }
            else
            {
                logInfo = "Connection string configured (format not recognized)";
            }

            if (logger != null)
            {
                logger.LogInformation("Database connection configured: {ConnectionInfo}", logInfo);
            }
            else
            {
                Console.WriteLine($"Database connection configured: {logInfo}");
            }
        }
        catch (Exception ex)
        {
            var warningMessage = $"Failed to log connection info: {ex.Message}";
            if (logger != null)
            {
                logger.LogWarning(ex, "Failed to log connection info");
            }
            else
            {
                Console.WriteLine($"Warning: {warningMessage}");
            }
        }
    }
}

