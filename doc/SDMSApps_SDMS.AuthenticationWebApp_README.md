# SDMS Authentication Web App

A hybrid authentication service with .NET 8 backend and Angular 18 frontend, supporting both local and external authentication (Auth0, Google) with automatic fallback.

## Architecture

- **Backend**: .NET 8 Minimal APIs with OpenIddict, ASP.NET Core Identity, PostgreSQL
- **Frontend**: Angular 18 with angular-oauth2-oidc, standalone components
- **Database**: PostgreSQL with EF Core
- **Authentication**: OpenIddict (OIDC/OAuth2), External providers (Auth0, Google)

## Features

- ✅ Local user registration and login
- ✅ External authentication (Auth0, Google)
- ✅ Automatic user creation from external providers
- ✅ Fallback to local authentication if external provider fails
- ✅ JWT token generation with refresh tokens
- ✅ PKCE support for enhanced security
- ✅ User profile management
- ✅ Webhook support for real-time user sync
- ✅ Docker support with docker-compose

## Project Structure

```
SDMS.AuthenticationWebApp/
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   └── ApplicationUser.cs
├── Services/
│   ├── IExternalAuthService.cs
│   ├── ExternalAuthService.cs
│   └── TokenService.cs
├── Controllers/
│   ├── AccountController.cs
│   └── WebhookController.cs
├── ClientApp/ (Angular Frontend)
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   ├── login/
│   │   │   │   ├── register/
│   │   │   │   ├── profile/
│   │   │   │   └── auth-callback/
│   │   │   └── services/
│   │   │       └── auth.service.ts
│   │   └── environments/
│   ├── angular.json
│   └── package.json
├── Program.cs
├── Dockerfile
└── README.md
```

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- PostgreSQL 15+
- Docker and Docker Compose (optional)

## Setup

### 1. Database Setup

#### Option A: Using Docker Compose (Recommended)

```bash
cd SDMSApps
docker-compose up -d postgres
```

#### Option B: Local PostgreSQL

Ensure PostgreSQL is running and create a database:

```sql
CREATE DATABASE sdms_auth;
```

### 2. Configure Environment Variables

Set environment variables or update `appsettings.json` for local development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=sdms_auth;Username=postgres;Password=postgres;Port=5432"
  },
  "ExternalAuth": {
    "Auth0": {
      "Domain": "your-domain.auth0.com",
      "ClientId": "your-auth0-client-id",
      "ClientSecret": "your-auth0-client-secret"
    },
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret"
    },
    "RedirectUri": "http://localhost:4200/auth-callback"
  }
}
```

Or set environment variables:

```bash
export POSTGRES_CONNECTION="Host=localhost;Database=sdms_auth;Username=postgres;Password=postgres"
export ExternalAuth__Auth0__Domain="your-domain.auth0.com"
export ExternalAuth__Auth0__ClientId="your-client-id"
export ExternalAuth__Auth0__ClientSecret="your-client-secret"
export ExternalAuth__Google__ClientId="your-google-client-id"
export ExternalAuth__Google__ClientSecret="your-google-client-secret"
```

### 3. Run Database Migrations

```bash
cd SDMSApps/SDMS.AuthenticationWebApp
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run Backend

```bash
cd SDMSApps/SDMS.AuthenticationWebApp
dotnet restore
dotnet run
```

The backend will be available at `https://localhost:7001`

### 5. Setup Angular Frontend

```bash
cd SDMSApps/SDMS.AuthenticationWebApp/ClientApp
npm install
npm start
```

The frontend will be available at `http://localhost:4200`

## API Endpoints

### Authentication

- `POST /account/login` - Login with email/password or external provider
- `POST /account/register` - Register new user
- `GET /account/userinfo` - Get user profile (requires authentication)
- `GET /.well-known/openid-configuration` - OpenIddict discovery document
- `POST /connect/token` - Token endpoint (OpenIddict)
- `GET /connect/authorize` - Authorization endpoint (OpenIddict)
- `POST /connect/logout` - Logout endpoint

### Webhooks

- `POST /webhook/external-user` - Sync external user data

## Authentication Flows

### 1. Local Login

```http
POST /account/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### 2. External Login (Auth0/Google)

The frontend initiates OAuth flow:

1. User clicks "Sign in with Auth0/Google"
2. Redirects to authorization endpoint: `/connect/authorize?client_id=sdms_frontend&response_type=code&scope=openid profile email&redirect_uri=http://localhost:4200/auth-callback`
3. After external authentication, callback at `/auth-callback` receives authorization code
4. Frontend exchanges code for tokens via `/connect/token`
5. User profile is fetched from `/account/userinfo`

### 3. Fallback Flow

If external provider fails (network error, 5xx, etc.):
1. System attempts local authentication
2. Checks user credentials via `UserManager.CheckPasswordAsync`
3. Returns local JWT if credentials match

## Testing

### Test Local Login

```bash
curl -X POST https://localhost:7001/account/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","displayName":"Test User"}'

curl -X POST https://localhost:7001/account/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

### Test External Login

1. Configure Auth0 or Google credentials in `appsettings.json` or via environment variables
2. Open frontend at `http://localhost:4200`
3. Click "Sign in with Auth0" or "Sign in with Google"
4. Complete external authentication
5. Verify user is created/updated in database

### Verify Database

```sql
SELECT * FROM "AspNetUsers";
SELECT * FROM "OpenIddictApplications";
SELECT * FROM "OpenIddictAuthorizations";
```

## Docker Deployment

### Build and Run with Docker Compose

```bash
cd SDMSApps
docker-compose up -d
```

This will:
- Start PostgreSQL container
- Build and start the authentication service
- Database will be automatically initialized

### Environment Variables for Docker

Set in `docker-compose.yml` or `.env` file:

```env
AUTH0_DOMAIN=your-domain.auth0.com
AUTH0_CLIENT_ID=your-client-id
AUTH0_CLIENT_SECRET=your-client-secret
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
```

## External Provider Configuration

### Auth0 Setup

1. Create Auth0 Application (Single Page Application)
2. Configure Allowed Callback URLs: `http://localhost:4200/auth-callback`
3. Configure Allowed Logout URLs: `http://localhost:4200`
4. Copy Domain, Client ID, and Client Secret to configuration

### Google Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create OAuth 2.0 Client ID
3. Configure Authorized redirect URIs: `http://localhost:4200/auth-callback`
4. Copy Client ID and Client Secret to configuration

## Security Considerations

- ✅ PKCE (Proof Key for Code Exchange) enabled
- ✅ HTTPS redirection in production
- ✅ Secure token storage (sessionStorage)
- ✅ CORS configured for frontend origin only
- ✅ Environment variables for secrets
- ✅ RSA signing keys for JWT
- ✅ Refresh token support

## Troubleshooting

### Database Connection Issues

- Verify PostgreSQL is running
- Check connection string in `appsettings.json` or environment variables
- Ensure database exists

### External Auth Not Working

- Verify client ID and secret are correct
- Check redirect URIs match in provider configuration
- Review logs for detailed error messages

### Token Issues

- Check OpenIddict client configuration
- Verify signing certificates are generated
- Review CORS settings

## Development Notes

- Development certificates are used for JWT signing (not for production)
- Email confirmation is disabled in development
- CORS allows `http://localhost:4200` for development
- Swagger UI available at `/swagger` in development mode

## Production Deployment

1. **Use Production Signing Keys**: Generate RSA keys and configure in `Auth:SigningKeyPath`
2. **Enable HTTPS**: Configure proper SSL certificates
3. **Set Strong Passwords**: Update password requirements in `Program.cs`
4. **Configure CORS**: Update allowed origins in production
5. **Enable Email Confirmation**: Set `RequireConfirmedEmail = true`
6. **Secure Secrets**: Use Azure Key Vault, AWS Secrets Manager, or similar
7. **Database Backups**: Configure automated backups for PostgreSQL

## License

MIT

