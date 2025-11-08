# SDMS - Service Delivery Management System

A microservices-based enterprise application for service delivery management, combining features from Zomato, Blinkit, Dunzo, and FedEx.

## Overview

This is a comprehensive microservices-based platform built with:
- **Backend**: .NET Core 8.0 (C#) with OpenIddict for authentication
- **Frontend**: Angular 18 (TypeScript) with server-side rendering support
- **Architecture**: API Gateway pattern using Ocelot
- **Databases**: MySQL, MongoDB, PostgreSQL, FerretDB
- **Authentication**: OAuth 2.0 / OpenID Connect via OpenIddict

## Project Structure

```
SDMSApps/
├── Web Applications
│   ├── SDMS.B2CWebApp              # Customer-facing web app
│   ├── SDMS.B2BWebApp              # Business-to-business portal
│   ├── SDMS.BackOfficeWebApp       # Administrative portal
│   ├── SDMS.DeliveryPartnerWebApp  # Delivery partner portal
│   ├── SDMS.VendorWebApp           # Vendor management portal
│   └── SDMS.AuthenticationWebApp   # Authentication web app
│
├── API Services
│   ├── SDMS.GatewayApi             # API Gateway (Ocelot)
│   ├── SDMS.AuthenticationApi      # Authentication service
│   ├── SDMS.CatalogApi             # Catalog/product management
│   └── SDMS.ContentManagementApi   # Content management
│
├── Data Layer
│   ├── SDMS.DL.MySql               # MySQL data access
│   ├── SDMS.DL.MongoDB             # MongoDB data access
│   ├── SDMS.DL.FerretDB            # FerretDB data access
│   └── SDMS.DL.PostgreSQL          # PostgreSQL data access
│
├── Business Logic
│   ├── SDMS.BL.Common              # Common business logic
│   └── SDMS.ContentManagementApi.BL
│
└── Shared Libraries
    ├── SDMS.Models                 # Domain models
    ├── SDMS.ViewModels             # View models
    ├── SDMS.Common.Infra           # Common infrastructure
    └── ClientAppLibrary            # Shared Angular library
```

## Quick Start

### Prerequisites

- **.NET SDK 8.0** - For backend projects
- **.NET SDK 3.1** - For older projects (B2BWebApp)
- **Node.js 18.x** - For Angular frontend
- **npm** - Package manager

### Building the Solution

```powershell
# Restore dependencies
dotnet restore SDMSApps/SDMSApps.sln

# Build solution
dotnet build SDMSApps/SDMSApps.sln

# Build specific project
cd SDMSApps/SDMS.AuthenticationWebApp
dotnet build
```

### Running Applications

```powershell
# Run Authentication WebApp
cd SDMSApps/SDMS.AuthenticationWebApp
dotnet run

# Run B2C WebApp (Frontend)
cd SDMSApps/SDMS.B2CWebApp/ClientApp
npm install
npm start
```

## Documentation

### Main Documentation

- **[PROJECT_HISTORY.md](PROJECT_HISTORY.md)** - Complete project history, renaming process, build issues, and verification
- **[.github/workflows/WORKFLOWS_README.md](.github/workflows/WORKFLOWS_README.md)** - GitHub Actions workflows documentation

### Project-Specific Documentation

Each project may have its own README.md file:
- `SDMSApps/SDMS.AuthenticationWebApp/README.md` - Authentication WebApp documentation
- `SDMSApps/SDMS.B2CWebApp/README_DEPLOYMENT.md` - B2C WebApp deployment guide
- `SDMSApps/SDMS.AuthenticationWebApp/README_DEPLOYMENT.md` - Authentication WebApp deployment guide

## CI/CD

### GitHub Actions Workflows

- **CI Workflows** - Run on every push and pull request
- **Deployment Workflows** - Deploy to production on `release` branch
- **Error Handler** - Automatically detects and reports build failures

See [.github/workflows/WORKFLOWS_README.md](.github/workflows/WORKFLOWS_README.md) for detailed documentation.

### Deployment

- **Frontend (B2CWebApp)** → Vercel
- **Backend (AuthenticationWebApp)** → Railway

## Scripts

All PowerShell scripts are located in the `scripts/` folder:

- `RENAME_PROJECTS.ps1` - Rename project folders and files
- `UPDATE_NAMESPACES.ps1` - Update namespaces in code files
- `RENAME_ALL.ps1` - Complete renaming script
- `UPDATE_ALL_NAMESPACES.ps1` - Update all namespaces
- `UPDATE_CONFIG_FILES.ps1` - Update configuration files

## Development

### Local Development

1. Clone the repository
2. Install prerequisites (.NET SDK, Node.js)
3. Restore dependencies: `dotnet restore`
4. Build solution: `dotnet build`
5. Run applications: `dotnet run`

### Contributing

1. Create a feature branch
2. Make your changes
3. Run tests and build
4. Create a pull request

## Technology Stack

- **.NET Core 8.0** - Backend framework
- **Angular 18** - Frontend framework
- **OpenIddict** - Authentication server
- **Ocelot** - API Gateway
- **Entity Framework Core** - ORM
- **MySQL** - Relational database
- **MongoDB** - NoSQL database
- **PostgreSQL** - Additional database option

## License

[Your License Here]

## Support

For issues and questions:
- Check [PROJECT_HISTORY.md](PROJECT_HISTORY.md) for common issues and solutions
- Check [.github/workflows/WORKFLOWS_README.md](.github/workflows/WORKFLOWS_README.md) for workflow issues
- Create an issue in the repository

---

**Last Updated:** 2024
**Status:** ✅ Active Development
