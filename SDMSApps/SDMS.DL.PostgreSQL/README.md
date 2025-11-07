# SDMS.DL.PostgreSQL

PostgreSQL Data Access Layer for SDMS platform.

## Overview

This project provides a generic repository pattern implementation for PostgreSQL database operations, similar to the existing MySQL and MongoDB data access layers. It supports CRUD operations and querying with filtering and ordering.

## Project Structure

```
SDMS.DL.PostgreSQL/
├── Interface/
│   ├── IPostgreSqlDBOperations.cs          - Base interface for query operations
│   └── IPostgreSqlDBOperationsEntity.cs    - Interface for entity CRUD operations
├── Implementation/
│   ├── PostgreSqlDBOperations.cs           - Base implementation for query operations
│   └── PostgreSqlDBOperationsEntity.cs     - Entity CRUD operations implementation
├── Helpers/
│   └── Helpers.cs                          - Type conversion helpers for PostgreSQL
└── SDMS.DL.PostgreSQL.csproj
```

## Features

- **Generic Repository Pattern**: Type-safe operations using generics
- **CRUD Operations**: Insert, Update, Delete, Get, and GetListByFilter
- **Filtering**: Support for complex filter conditions with logical operators (AND, OR)
- **Ordering**: Support for ordering by multiple columns
- **Type Safety**: Automatic type conversion from C# types to PostgreSQL types
- **Async Operations**: All database operations are asynchronous

## Usage

### Configuration

Add PostgreSQL connection string to `appsettings.json`:

```json
{
  "PostgreSqlSettings": {
    "ConnectionString": "Host=localhost;Database=mydb;Username=postgres;Password=password"
  }
}
```

### Basic Usage

#### 1. Query Operations (IPostgreSqlDBOperations)

```csharp
using SDMS.DL.PostgreSQL.Interface;
using SDMS.DL.PostgreSQL.Implementation;
using SDMS.Common.Infra.Models;

// Inject IConfiguration
public class MyService
{
    private readonly IPostgreSqlDBOperations<MyModel> _dbOperations;
    
    public MyService(IConfiguration configuration)
    {
        _dbOperations = new PostgreSqlDBOperations<MyModel>(configuration);
    }
    
    public async Task<IList<MyModel>> GetUsers()
    {
        var filters = new List<FilterModel>
        {
            new FilterModel 
            { 
                Key = "IsActive", 
                Value = true,
                ConditionalOperator = ConditionalOperator.Equal,
                LogicalOperator = LogicalOperator.And
            }
        };
        
        var orderBy = new List<OrderByModel>
        {
            new OrderByModel 
            { 
                Collumn = "CreatedDate", 
                Direction = OrderByOperator.Descending 
            }
        };
        
        var result = await _dbOperations.GetListByFilter(filters, orderBy);
        return result.Result;
    }
}
```

#### 2. Entity Operations (IPostgreSqlDBOperationsEntity)

```csharp
using SDMS.DL.PostgreSQL.Interface;
using SDMS.DL.PostgreSQL.Implementation;
using SDMS.Common.Infra.Models;

public class MyEntity : Entity<int>
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class MyEntityService
{
    private readonly IPostgreSqlDBOperationsEntity<MyEntity, int> _dbOperations;
    
    public MyEntityService(IConfiguration configuration)
    {
        _dbOperations = new PostgreSqlDBOperationsEntity<MyEntity, int>(configuration);
    }
    
    // Insert
    public async Task<int> CreateEntity(MyEntity entity)
    {
        var result = await _dbOperations.Insert(entity);
        return result.Result;
    }
    
    // Get by ID
    public async Task<MyEntity> GetEntity(int id)
    {
        var result = await _dbOperations.Get(id);
        return result.Result;
    }
    
    // Update
    public async Task<bool> UpdateEntity(MyEntity entity)
    {
        var result = await _dbOperations.Update(entity);
        return result.Result;
    }
    
    // Delete
    public async Task<bool> DeleteEntity(int id)
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

[TableName("users")]  // Optional: specify table name
public class User : Entity<int>
{
    [Key]  // Optional: mark primary key (default is "Id")
    public override int Id { get; set; }
    
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}
```

## Supported Data Types

The helper class automatically converts C# types to PostgreSQL types:

| C# Type | PostgreSQL Type |
|---------|----------------|
| `byte` | `Smallint` |
| `int` | `Integer` |
| `long` | `Bigint` |
| `float` | `Real` |
| `double` | `Double` |
| `string` | `Varchar` |
| `DateTime` | `Timestamp` |
| `Guid` | `Uuid` |
| `decimal` | `Numeric` |
| `bool` | `Boolean` |
| `byte[]` | `Bytea` |

## PostgreSQL-Specific Features

1. **Quoted Identifiers**: Column and table names are automatically quoted using double quotes (`"column_name"`) to handle case sensitivity
2. **RETURNING Clause**: Uses PostgreSQL's `RETURNING` clause for INSERT operations instead of `LAST_INSERT_ID()`
3. **Parameterized Queries**: All queries use parameterized statements to prevent SQL injection

## Dependencies

- **.NET Core 3.1**
- **Npgsql 4.1.9** - PostgreSQL ADO.NET provider
- **SDMS.Common.Infra** - Common infrastructure and models
- **Microsoft.Extensions.Configuration.Abstractions 3.1.7**

## Differences from MySQL Implementation

1. **Connection**: Uses `NpgsqlConnection` instead of `MySqlConnection`
2. **Command**: Uses `NpgsqlCommand` instead of `MySqlCommand`
3. **Type Mapping**: Uses `NpgsqlDbType` instead of `MySqlDbType`
4. **SQL Syntax**: Uses PostgreSQL-specific syntax (quoted identifiers, RETURNING clause)
5. **Configuration Key**: Uses `PostgreSqlSettings:ConnectionString` instead of `MySqlSettings:ConnectionString`

## Integration

To use this data access layer in your API project:

1. Add project reference:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\SDMS.DL.PostgreSQL\SDMS.DL.PostgreSQL.csproj" />
   </ItemGroup>
   ```

2. Register in DI container (if needed):
   ```csharp
   services.AddScoped(typeof(IPostgreSqlDBOperations<>), typeof(PostgreSqlDBOperations<>));
   services.AddScoped(typeof(IPostgreSqlDBOperationsEntity<,>), typeof(PostgreSqlDBOperationsEntity<,>));
   ```

## Notes

- The implementation follows the same pattern as `SDMS.DL.MySql` for consistency
- All operations return `BaseResult<T>` which includes error handling
- Null values are handled using `DBNull.Value`
- The implementation is thread-safe and uses async/await throughout

