# Multi-Service Delivery Platform - Project Structure

## Overview

This document outlines the complete project structure for the multi-service delivery platform combining features from Zomato, Blinkit, Dunzo, and FedEx.

## Solution Structure

```
SDSMApps.sln
├── Web Applications (Frontend + Backend Hosting)
│   ├── SDSM.EndUserWebApp          [EXISTING] - Customer app
│   ├── SDSM.VendorWebApp           [EXISTING] - Vendor portal
│   ├── SDSM.DeliveryPartnerWebApp  [EXISTING] - Delivery partner app
│   ├── SDSM.BackOfficeWebApp       [EXISTING] - Admin portal
│   └── SDSM.B2BWebApp              [EXISTING] - B2B portal
│
├── API Gateway
│   └── SDSM.GatewayApi             [EXISTING] - Ocelot API Gateway
│
├── Core APIs (Microservices)
│   ├── SDSM.AuthenticationApi      [EXISTING] - Identity Server 4
│   ├── SDSM.CatalogApi             [EXISTING] - Catalog/Product API
│   ├── SDSM.ContentManagementApi   [EXISTING] - CMS API
│   │
│   ├── SDSM.OrderApi               [NEW] - Order management
│   ├── SDSM.DeliveryApi            [NEW] - Delivery partner management
│   ├── SDSM.VendorApi              [NEW] - Vendor management
│   ├── SDSM.LogisticsApi           [NEW] - Logistics partner management
│   ├── SDSM.PaymentApi             [NEW] - Payment processing
│   ├── SDSM.NotificationApi        [NEW] - Notifications (SMS/Email/Push)
│   └── SDSM.RatingApi              [NEW] - Ratings and reviews
│
├── Business Logic Layer (BL)
│   ├── SDSM.BL.Common              [EXISTING] - Common business logic
│   ├── SDSM.ContentManagementApi.BL [EXISTING]
│   │
│   ├── SDSM.OrderApi.BL            [NEW]
│   ├── SDSM.DeliveryApi.BL         [NEW]
│   ├── SDSM.VendorApi.BL           [NEW]
│   ├── SDSM.LogisticsApi.BL        [NEW]
│   ├── SDSM.PaymentApi.BL          [NEW]
│   ├── SDSM.NotificationApi.BL     [NEW]
│   └── SDSM.RatingApi.BL           [NEW]
│
├── Data Access Layer (DL)
│   ├── SDSM.DL.MySql               [EXISTING] - MySQL repository
│   └── SDSM.DL.MongoDB             [EXISTING] - MongoDB repository
│
├── Shared Libraries
│   ├── SDSM.Models                 [EXISTING] - Domain models
│   ├── SDSM.ViewModels             [EXISTING] - View models
│   ├── SDSM.Common.Infra           [EXISTING] - Common infrastructure
│   └── ClientAppLibrary            [EXISTING] - Shared Angular library
│
└── Infrastructure Projects (Optional)
    ├── SDSM.MessageQueue           [NEW] - Message queue abstraction
    ├── SDSM.Cache                  [NEW] - Cache abstraction (Redis)
    └── SDSM.RealTime               [NEW] - SignalR hub library
```

## Project Details

### 1. API Projects Structure

Each API project follows this structure:
```
SDSM.{Service}Api/
├── Controllers/
│   └── {Service}Controller.cs
├── Program.cs
├── Startup.cs
├── appsettings.json
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json
└── SDSM.{Service}Api.csproj
```

### 2. Business Logic Projects Structure

Each BL project follows this structure:
```
SDSM.{Service}Api.BL/
├── Interface/
│   └── I{Service}.cs
├── Implementation/
│   └── {Service}.cs
├── Mapper/
│   ├── Request/
│   └── Response/
└── SDSM.{Service}Api.BL.csproj
```

### 3. New Projects to Create

#### A. Order Management
- **SDSM.OrderApi** - Order CRUD, status management, tracking
- **SDSM.OrderApi.BL** - Order business logic, validation, workflow

#### B. Delivery Management
- **SDSM.DeliveryApi** - Delivery partner management, assignment, tracking
- **SDSM.DeliveryApi.BL** - Assignment algorithm, route optimization

#### C. Vendor Management
- **SDSM.VendorApi** - Vendor CRUD, menu/inventory management
- **SDSM.VendorApi.BL** - Vendor business logic, validation

#### D. Logistics Management
- **SDSM.LogisticsApi** - Logistics partner management, shipments
- **SDSM.LogisticsApi.BL** - Logistics business logic, integration

#### E. Payment Processing
- **SDSM.PaymentApi** - Payment processing, refunds, wallets
- **SDSM.PaymentApi.BL** - Payment gateway integration, validation

#### F. Notifications
- **SDSM.NotificationApi** - SMS, Email, Push notifications
- **SDSM.NotificationApi.BL** - Notification templates, sending logic

#### G. Ratings & Reviews
- **SDSM.RatingApi** - Ratings, reviews, feedback
- **SDSM.RatingApi.BL** - Rating aggregation, validation

## Project Dependencies

### Common Dependencies (All API Projects)
- Microsoft.AspNetCore.App (3.1 or 6.0)
- SDSM.Common.Infra
- SDSM.Models
- SDSM.ViewModels

### API-Specific Dependencies

#### OrderApi
- SDSM.OrderApi.BL
- SDSM.DL.MySql
- SDSM.DL.MongoDB (for tracking)
- SignalR (for real-time updates)

#### DeliveryApi
- SDSM.DeliveryApi.BL
- SDSM.DL.MySql
- SDSM.DL.MongoDB (for location tracking)
- SignalR (for real-time location)

#### VendorApi
- SDSM.VendorApi.BL
- SDSM.DL.MySql

#### LogisticsApi
- SDSM.LogisticsApi.BL
- SDSM.DL.MySql
- External API integration libraries

#### PaymentApi
- SDSM.PaymentApi.BL
- SDSM.DL.MySql
- Payment gateway SDKs (Razorpay, Stripe, etc.)

#### NotificationApi
- SDSM.NotificationApi.BL
- SDSM.DL.MongoDB (for notification logs)
- SMS/Email service providers
- Firebase Admin SDK (for push notifications)

#### RatingApi
- SDSM.RatingApi.BL
- SDSM.DL.MySql

## Folder Structure Template

### API Project Template
```
SDSM.{Service}Api/
├── Controllers/
│   └── {Service}Controller.cs
├── Middleware/
│   └── (if needed)
├── Filters/
│   └── (if needed)
├── Program.cs
├── Startup.cs
├── appsettings.json
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json
└── SDSM.{Service}Api.csproj
```

### BL Project Template
```
SDSM.{Service}Api.BL/
├── Interface/
│   └── I{Service}.cs
├── Implementation/
│   └── {Service}.cs
├── Mapper/
│   ├── Request/
│   │   └── {Service}RequestMapper.cs
│   └── Response/
│       └── {Service}ResponseMapper.cs
├── Validators/
│   └── (if needed)
└── SDSM.{Service}Api.BL.csproj
```

## Database Strategy

### MySQL (Relational Data)
- Orders
- OrderItems
- Vendors
- Services
- Customers
- DeliveryPartners
- LogisticsPartners
- Payments
- Ratings
- Addresses

### MongoDB (Document Store)
- OrderTracking (real-time tracking data)
- DeliveryLocations (real-time location updates)
- Notifications (notification logs)
- AuditLogs

## Next Steps

1. **Create project folders** - Set up directory structure
2. **Create .csproj files** - Define project files with dependencies
3. **Update solution file** - Add new projects to SDSMApps.sln
4. **Create basic structure** - Program.cs, Startup.cs, Controllers
5. **Set up dependencies** - Configure project references
6. **Create domain models** - Add models to SDSM.Models project
7. **Update Gateway** - Add routes for new APIs

## Implementation Order

### Phase 1: Foundation (Week 1)
1. Create all project folders
2. Create .csproj files
3. Update solution file
4. Create basic Program.cs and Startup.cs for each API
5. Set up project references

### Phase 2: Domain Models (Week 1-2)
1. Create domain models in SDSM.Models
2. Create view models in SDSM.ViewModels
3. Update enums in SDSM.Models.Constants

### Phase 3: Data Layer (Week 2)
1. Create database tables (MySQL)
2. Create MongoDB collections
3. Update data access layer if needed

### Phase 4: Business Logic (Week 2-3)
1. Create BL interfaces
2. Implement BL classes
3. Create mappers

### Phase 5: API Implementation (Week 3-4)
1. Create controllers
2. Implement endpoints
3. Add validation
4. Add error handling

### Phase 6: Integration (Week 4-5)
1. Update Gateway API routes
2. Integrate services
3. Test end-to-end flows

### Phase 7: Real-time Features (Week 5-6)
1. Set up SignalR
2. Implement real-time updates
3. Add location tracking

### Phase 8: Testing & Deployment (Week 6+)
1. Unit tests
2. Integration tests
3. Performance testing
4. Deployment

## Notes

- All projects use .NET Core 3.1 (consider upgrading to .NET 6/7)
- Follow existing code patterns and conventions
- Use dependency injection throughout
- Implement proper error handling
- Add logging (Serilog, NLog, etc.)
- Add health checks
- Add API versioning
- Add Swagger/OpenAPI documentation

