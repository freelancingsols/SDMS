# SDMS.DL.FerretDB

FerretDB Data Access Layer for SDMS platform.

## Overview

This project provides a generic repository pattern implementation for FerretDB database operations. FerretDB is a MongoDB-compatible database, so this implementation uses the MongoDB driver and follows the same pattern as the MongoDB data access layer.

## Project Structure

```
SDMS.DL.FerretDB/
├── Interface/
│   ├── IFerretDBConnection.cs              - Connection interface
│   ├── IConnectionContext.cs               - Collection context interface
│   ├── IFerretDBOperations.cs              - Base interface for query operations
│   └── IFerretDBOperationsEntity.cs        - Interface for entity CRUD operations
├── Implementation/
│   ├── FerretDBConnection.cs               - Connection implementation
│   ├── ConnectionContext.cs                - Collection context implementation
│   ├── FerretDBOperations.cs               - Base implementation for query operations
│   └── FerretDBOperationsEntity.cs         - Entity CRUD operations implementation
└── SDMS.DL.FerretDB.csproj
```

## Features

- **MongoDB-Compatible**: Uses MongoDB driver, fully compatible with FerretDB
- **Generic Repository Pattern**: Type-safe operations using generics
- **CRUD Operations**: Insert, Update, Delete, Get, and GetListByFilter
- **Filtering**: Support for LINQ-based filtering
- **Ordering**: Support for ordering by multiple columns
- **Async Operations**: All database operations are asynchronous

## About FerretDB

FerretDB is an open-source alternative to MongoDB that provides MongoDB-compatible APIs. It stores data in PostgreSQL, making it a great choice for teams that want MongoDB compatibility with PostgreSQL as the backend storage.

## Usage

### Configuration

Add FerretDB connection string to `appsettings.json`:

```json
{
  "FerretDBSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DataBaseName": "mydb"
  }
}
```

### Basic Usage

#### 1. Setup Connection and Context

```csharp
using SDMS.DL.FerretDB.Interface;
using SDMS.DL.FerretDB.Implementation;
using Microsoft.Extensions.Configuration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register FerretDB connection
        services.AddSingleton<IFerretDBConnection>(sp => 
            new FerretDBConnection(configuration));
        
        // Register context (scoped per request)
        services.AddScoped(typeof(IConnectionContext<>), typeof(ConnectionContext<>));
        
        // Register operations
        services.AddScoped(typeof(IFerretDBOperations<>), typeof(FerretDBOperations<>));
        services.AddScoped(typeof(IFerretDBOperationsEntity<,>), typeof(FerretDBOperationsEntity<,>));
    }
}
```

#### 2. Query Operations (IFerretDBOperations)

```csharp
using SDMS.DL.FerretDB.Interface;
using SDMS.Common.Infra.Models;

public class MyService
{
    private readonly IFerretDBOperations<MyModel> _dbOperations;
    
    public MyService(IFerretDBOperations<MyModel> dbOperations)
    {
        _dbOperations = dbOperations;
    }
    
    public async Task<IList<MyModel>> GetActiveUsers()
    {
        var orderBy = new List<OrderByPredicateModel<MyModel>>
        {
            new OrderByPredicateModel<MyModel>
            {
                Collumn = x => x.CreatedDate,
                Direction = OrderByOperator.Descending
            }
        };
        
        var result = await _dbOperations.GetListByFilter(
            x => x.IsActive == true, 
            orderBy);
        
        return result.Result;
    }
}
```

#### 3. Entity Operations (IFerretDBOperationsEntity)

```csharp
using SDMS.DL.FerretDB.Interface;
using SDMS.Common.Infra.Models;

[CollectionName("users")]  // Optional: specify collection name
public class User : Entity<Guid>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}

public class UserService
{
    private readonly IFerretDBOperationsEntity<User, Guid> _dbOperations;
    
    public UserService(IFerretDBOperationsEntity<User, Guid> dbOperations)
    {
        _dbOperations = dbOperations;
    }
    
    // Insert
    public async Task<bool> CreateUser(User user)
    {
        var result = await _dbOperations.Insert(user);
        return result.Result;
    }
    
    // Get by ID
    public async Task<User> GetUser(Guid id)
    {
        var result = await _dbOperations.Get(id);
        return result.Result;
    }
    
    // Update
    public async Task<bool> UpdateUser(User user)
    {
        var result = await _dbOperations.Update(user);
        return result.Result;
    }
    
    // Delete
    public async Task<bool> DeleteUser(Guid id)
    {
        var result = await _dbOperations.Delete(id);
        return result.Result;
    }
}
```

### Model Definition

Your entity classes should inherit from `Entity<T>` where `T` is the key type:

```csharp
using SDMS.Common.Infra.Models;
using SDMS.Common.Infra.Attributes;

[CollectionName("products")]  // Optional: specify collection name
public class Product : Entity<string>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

## FerretDB vs MongoDB

### Similarities
- Same connection string format
- Same query API (MongoDB driver)
- Same document structure
- Same collection and database concepts

### Differences
- **Storage Backend**: FerretDB uses PostgreSQL, MongoDB uses its own storage engine
- **Configuration**: Uses `FerretDBSettings` instead of `MongoSettings`
- **Performance**: May have different performance characteristics due to PostgreSQL backend

## Dependencies

- **.NET Core 3.1**
- **MongoDB.Driver 2.11.1** - MongoDB ADO.NET provider (compatible with FerretDB)
- **SDMS.Common.Infra** - Common infrastructure and models
- **Microsoft.Extensions.Configuration.Abstractions 3.1.7**

## Integration

To use this data access layer in your API project:

1. Add project reference:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\SDMS.DL.FerretDB\SDMS.DL.FerretDB.csproj" />
   </ItemGroup>
   ```

2. Register in DI container:
   ```csharp
   services.AddSingleton<IFerretDBConnection>(sp => 
       new FerretDBConnection(configuration));
   services.AddScoped(typeof(IConnectionContext<>), typeof(ConnectionContext<>));
   services.AddScoped(typeof(IFerretDBOperations<>), typeof(FerretDBOperations<>));
   services.AddScoped(typeof(IFerretDBOperationsEntity<,>), typeof(FerretDBOperationsEntity<,>));
   ```

## Notes

- The implementation follows the same pattern as `SDMS.DL.MongoDB` for consistency
- All operations return `BaseResult<T>` which includes error handling
- The implementation is thread-safe and uses async/await throughout
- FerretDB connection strings use the same format as MongoDB
- Collection names can be specified using `[CollectionName]` attribute or default to class name

## Benefits of Using FerretDB

1. **PostgreSQL Backend**: Leverage PostgreSQL's reliability and features
2. **MongoDB Compatibility**: Use existing MongoDB tools and libraries
3. **Open Source**: Free and open-source alternative
4. **SQL Access**: Can also access data directly via PostgreSQL if needed
5. **Cost Effective**: No licensing fees compared to MongoDB Enterprise

