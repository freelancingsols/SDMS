using SDMS.AuthenticationWebApp.Constants;

namespace SDMS.AuthenticationWebApp.Configuration;

/// <summary>
/// Handles deployment platform-specific configuration
/// Supports multiple platforms: Railway, Azure, AWS, Local Development, etc.
/// </summary>
public static class DeploymentConfiguration
{
    /// <summary>
    /// Gets the database connection string from SDMS_AuthenticationWebApp_ConnectionString environment variable.
    /// 
    /// CONVERTING RAILWAY DATABASE_URL FORMAT TO NPGQL FORMAT:
    /// -------------------------------------------------------------------------
    /// Railway provides DATABASE_URL in this format:
    ///   postgresql://username:password@host:port/database
    /// 
    /// Convert it to Npgsql format:
    ///   Host=host;Port=port;Database=database;Username=username;Password=password
    /// 
    /// Example conversion:
    ///   Railway format:  postgresql://postgres:password123@postgres.railway.internal:5432/railway
    ///   Npgsql format:   Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=password123
    /// 
    /// Step-by-step conversion:
    ///   1. Extract host from URL: postgres.railway.internal
    ///   2. Extract port (default 5432 if not specified)
    ///   3. Extract database name: railway
    ///   4. Extract username: postgres
    ///   5. Extract password: password123
    ///   6. Combine: Host=host;Port=port;Database=database;Username=username;Password=password
    /// 
    /// Note: This method also accepts URL format and will automatically convert it to Npgsql format.
    /// </summary>
    public static string GetDatabaseConnectionString(IConfiguration configuration, ILogger? logger = null)
    {
        // Only use SDMS_AuthenticationWebApp_ConnectionString - no fallbacks
        // This provides explicit control over the connection string
        
        var connectionString = configuration[ConfigurationKeys.ConnectionString];
        
        // Check if connection string is null or empty
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Determine if we're in a production/deployed environment
            var isProduction = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"))
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_PROJECT_ID"))
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_ENVIRONMENT"))
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_REGION"))
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DYNO")) // Heroku
                || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production"
                || (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development" 
                    && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT"))); // Railway/cloud platforms set PORT
            
            if (isProduction)
            {
                // Log available environment variables for debugging
                LogAvailableEnvironmentVariables(configuration, logger);
                
                var errorMessage = "SDMS_AuthenticationWebApp_ConnectionString environment variable is required. " +
                    "Please set this variable with your PostgreSQL connection string. " +
                    "If you have Railway's DATABASE_URL, convert it to Npgsql format: " +
                    "Railway format: postgresql://user:pass@host:port/db → " +
                    "Npgsql format: Host=host;Port=port;Database=db;Username=user;Password=pass";
                
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
            // Log that connection string was found
            if (logger != null)
            {
                logger.LogInformation("Database connection string found in SDMS_AuthenticationWebApp_ConnectionString");
            }
            else
            {
                Console.WriteLine("Database connection string found in SDMS_AuthenticationWebApp_ConnectionString");
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
                "Set SDMS_AuthenticationWebApp_ConnectionString environment variable.");
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
                $"Please provide a valid PostgreSQL connection string in SDMS_AuthenticationWebApp_ConnectionString with Host specified.");
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

    /// <summary>
    /// Logs available environment variables for debugging (without exposing sensitive data)
    /// </summary>
    private static void LogAvailableEnvironmentVariables(IConfiguration configuration, ILogger? logger)
    {
        var connectionStringKey = ConfigurationKeys.ConnectionString;
        var connectionStringValue = configuration[connectionStringKey];
        
        var message = string.IsNullOrWhiteSpace(connectionStringValue)
            ? $"SDMS_AuthenticationWebApp_ConnectionString is not set. Please set this environment variable with your PostgreSQL connection string."
            : $"SDMS_AuthenticationWebApp_ConnectionString is set (value hidden for security).";

        if (logger != null)
        {
            logger.LogDebug(message);
        }
        else
        {
            Console.WriteLine($"Debug: {message}");
        }
    }
}

