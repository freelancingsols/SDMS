using Microsoft.Extensions.Configuration;
using Npgsql;
using SDMS.Common.Infra.Attributes;
using SDMS.Common.Infra.Models;
using SDMS.DL.PostgreSQL.Interface;
using SDMS.DL.PostgreSQL.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.PostgreSQL.Implementation
{
    public class PostgreSqlDBOperationsEntity<T, U> : PostgreSqlDBOperations<T>, IPostgreSqlDBOperationsEntity<T, U> where T : Entity<U> where U : struct
    {
        public PostgreSqlDBOperationsEntity(IConfiguration configuration) : base(configuration)
        {
        }
        
        public async Task<BaseResult<bool>> Delete(U value)
        {
            BaseResult<bool> result;
            try
            {
                var querybuilder = string.Format("DELETE FROM \"{0}\" WHERE \"{1}\"=@{1};", tableName, keyCollumn);
                using var connection = new NpgsqlConnection(configuration["PostgreSqlSettings:ConnectionString"]);
                await connection.OpenAsync();
                using var cmd = new NpgsqlCommand(querybuilder, connection);
                
                var dbType = SDMS.DL.PostgreSQL.Helpers.Helpers.ConvertToNpgsqlDbType(typeof(U));
                if (dbType.HasValue)
                {
                    cmd.Parameters.AddWithValue("@" + keyCollumn, dbType.Value, value);
                }
                
                var sqlResult = await cmd.ExecuteNonQueryAsync();
                result = new BaseResult<bool>();
                result.Result = sqlResult != 0;
                
                return result;
            }
            catch (Exception ex)
            {
                result = new BaseResult<bool>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }

        public async Task<BaseResult<T>> Get(U value)
        {
            BaseResult<T> result;
            try
            {
                var collumnQuery = new StringBuilder();
                foreach (var pr in properties)
                {
                    collumnQuery.AppendFormat("\"{0}\",", pr.Name);
                }
                if (collumnQuery.Length > 0)
                {
                    collumnQuery.Remove(collumnQuery.Length - 1, 1);
                }
                
                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("SELECT {0} FROM \"{1}\" WHERE \"{2}\"=@{2}", collumnQuery.ToString(), tableName, keyCollumn);
                
                using var connection = new NpgsqlConnection(configuration["PostgreSqlSettings:ConnectionString"]);
                await connection.OpenAsync();
                using var cmd = new NpgsqlCommand(querybuilder.ToString(), connection);
                
                var dbType = SDMS.DL.PostgreSQL.Helpers.Helpers.ConvertToNpgsqlDbType(typeof(U));
                if (dbType.HasValue)
                {
                    cmd.Parameters.AddWithValue("@" + keyCollumn, dbType.Value, value);
                }
                
                using var sqlResult = await cmd.ExecuteReaderAsync();
                result = new BaseResult<T>();

                while (await sqlResult.ReadAsync())
                {
                    result.Result = (T)Activator.CreateInstance(typeof(T));
                    foreach (var pr in properties)
                    {
                        var valueFromDb = sqlResult[pr.Name];
                        if (valueFromDb != DBNull.Value)
                        {
                            pr.SetValue(result.Result, Convert.ChangeType(valueFromDb, pr.PropertyType));
                        }
                    }
                    break;
                }
                return result;
            }
            catch (Exception ex)
            {
                result = new BaseResult<T>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }

        public async Task<BaseResult<U>> Insert(T request)
        {
            BaseResult<U> result;
            try
            {
                var collumnQuery = new StringBuilder();
                var valuesQuery = new StringBuilder();
                var parameters = new List<(string name, object value, PropertyInfo property)>();
                
                foreach (var pr in properties)
                {
                    if (pr.Name.Equals(keyCollumn, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    collumnQuery.AppendFormat("\"{0}\",", pr.Name);
                    valuesQuery.AppendFormat("@{0},", pr.Name);
                    parameters.Add((pr.Name, pr.GetValue(request), pr));
                }
                
                if (collumnQuery.Length > 0)
                {
                    collumnQuery.Remove(collumnQuery.Length - 1, 1);
                }
                if (valuesQuery.Length > 0)
                {
                    valuesQuery.Remove(valuesQuery.Length - 1, 1);
                }

                // PostgreSQL uses RETURNING clause instead of LAST_INSERT_ID()
                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("INSERT INTO \"{0}\" ({1}) VALUES ({2}) RETURNING \"{3}\";", 
                    tableName, collumnQuery.ToString(), valuesQuery.ToString(), keyCollumn);

                using var connection = new NpgsqlConnection(configuration["PostgreSqlSettings:ConnectionString"]);
                await connection.OpenAsync();
                using var cmd = new NpgsqlCommand(querybuilder.ToString(), connection);
                
                foreach (var param in parameters)
                {
                    var dbType = SDMS.DL.PostgreSQL.Helpers.Helpers.ConvertToNpgsqlDbType(param.property.PropertyType);
                    if (dbType.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@" + param.name, dbType.Value, param.value ?? DBNull.Value);
                    }
                }
                
                using var sqlResult = await cmd.ExecuteReaderAsync();
                result = new BaseResult<U>();
                
                while (await sqlResult.ReadAsync())
                {
                    var valueFromDb = sqlResult[keyCollumn];
                    if (valueFromDb != DBNull.Value)
                    {
                        result.Result = (U)Convert.ChangeType(valueFromDb, typeof(U));
                    }
                    break;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                result = new BaseResult<U>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }

        public async Task<BaseResult<bool>> Update(T request)
        {
            BaseResult<bool> result;
            try
            {
                var setQuery = new StringBuilder();
                var parameters = new List<(string name, object value, PropertyInfo property)>();
                
                foreach (var pr in properties)
                {
                    if (pr.Name.Equals(keyCollumn, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    setQuery.AppendFormat("\"{0}\"=@{1},", pr.Name, pr.Name);
                    parameters.Add((pr.Name, pr.GetValue(request), pr));
                }
                
                if (setQuery.Length > 0)
                {
                    setQuery.Remove(setQuery.Length - 1, 1);
                }
                
                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("UPDATE \"{0}\" SET {1} WHERE \"{2}\"=@{2}", tableName, setQuery.ToString(), keyCollumn);

                using var connection = new NpgsqlConnection(configuration["PostgreSqlSettings:ConnectionString"]);
                await connection.OpenAsync();
                using var cmd = new NpgsqlCommand(querybuilder.ToString(), connection);
                
                // Add all parameters including the key
                foreach (var param in parameters)
                {
                    var dbType = SDMS.DL.PostgreSQL.Helpers.Helpers.ConvertToNpgsqlDbType(param.property.PropertyType);
                    if (dbType.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@" + param.name, dbType.Value, param.value ?? DBNull.Value);
                    }
                }
                
                // Add the key parameter for WHERE clause
                var keyProperty = properties.First(x => x.Name.Equals(keyCollumn, StringComparison.OrdinalIgnoreCase));
                var keyValue = keyProperty.GetValue(request);
                var keyDbType = SDMS.DL.PostgreSQL.Helpers.Helpers.ConvertToNpgsqlDbType(typeof(U));
                if (keyDbType.HasValue)
                {
                    cmd.Parameters.AddWithValue("@" + keyCollumn, keyDbType.Value, keyValue);
                }
                
                var sqlResult = await cmd.ExecuteNonQueryAsync();
                result = new BaseResult<bool>();
                result.Result = sqlResult != 0;

                return result;
            }
            catch (Exception ex)
            {
                result = new BaseResult<bool>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }
    }
}

