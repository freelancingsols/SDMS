using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SDSM.Common.Infra.Attributes;
using SDSM.Common.Infra.Models;
using SDSM.DL.MySql.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDSM.DL.MySql.Implementation
{
    public class SqlDBOperationsEntity<T, U> : SqlDBOperations<T>, ISqlDBOperationsEntity<T, U> where T : Entity<U> where U : struct
    {
        
        public SqlDBOperationsEntity(IConfiguration configuration) : base(configuration)
        {
        }
        public async Task<BaseResult<bool>> Delete(U value)
        {
            BaseResult<bool> result;
            try
            {
                var querybuilder = string.Format("DELETE FROM {0} WHERE {1}=@{1};", tableName, keyCollumn);
                using var connection = new MySqlConnection(configuration["MySqlSettings:ConnectionString"]);
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = querybuilder.ToString();
                cmd.Prepare();
                cmd.Parameters.Add("@" + keyCollumn, Helpers.Helpers.ConvertToMySqlDbType(typeof(U)).Value).Value = value;
                cmd.CommandType = CommandType.Text;
                var sqlResult = await cmd.ExecuteNonQueryAsync();
                result = new BaseResult<bool>();
                if (sqlResult != 0)
                {
                    result.Result = true;

                }
                else
                {
                    result.Result = false;
                }
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
                    collumnQuery.AppendFormat("[{0}],", pr.Name);
                }
                collumnQuery.Remove(collumnQuery.Length - 1, 1);
                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("SELECT {0} FROM {1} WHERE {2}=@{2}", collumnQuery.ToString(), tableName, keyCollumn);
                using var connection = new MySqlConnection(configuration["MySqlSettings:ConnectionString"]);
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = querybuilder.ToString();
                cmd.Prepare();
                cmd.Parameters.Add("@" + keyCollumn, Helpers.Helpers.ConvertToMySqlDbType(typeof(U)).Value).Value = value;
                cmd.CommandType = CommandType.Text;
                var sqlResult = await cmd.ExecuteReaderAsync();
                result = new BaseResult<T>();

                while (sqlResult.Read())
                {
                    result.Result = (T)Activator.CreateInstance(typeof(T));
                    //TypeDescriptor.GetConverter(typeof(U)).ConvertFrom(sqlResult[keyCollumn]);
                    foreach (var pr in properties) 
                    {
                        pr.SetValue(result.Result,Convert.ChangeType(sqlResult[pr.Name], pr.PropertyType));
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
                foreach (var pr in properties)
                {
                    if (pr.Name.Equals(keyCollumn, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    collumnQuery.AppendFormat("[{0}],", pr.Name);
                    valuesQuery.AppendFormat("[{0}] =@{1},", pr.Name, pr.Name);
                }
                collumnQuery.Remove(collumnQuery.Length - 1, 1);
                valuesQuery.Remove(valuesQuery.Length - 1, 1);

                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2}); SELECT  LAST_INSERT_ID() {3};", tableName, collumnQuery.ToString(), valuesQuery.ToString(), keyCollumn);

                using var connection = new MySqlConnection(configuration["MySqlSettings:ConnectionString"]);
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = querybuilder.ToString();
                cmd.Prepare();
                foreach (var pr in properties)
                {
                    if (pr.Name.Equals(keyCollumn, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    cmd.Parameters.Add("@" + pr.Name, Helpers.Helpers.ConvertToMySqlDbType(pr.PropertyType).Value).Value = pr.GetValue(request);//map data type wise
                }
                cmd.CommandType = CommandType.Text;
                var sqlResult = await cmd.ExecuteReaderAsync();
                result = new BaseResult<U>();
                while (sqlResult.Read())
                {
                    //TypeDescriptor.GetConverter(typeof(U)).ConvertFrom(sqlResult[keyCollumn]);
                    result.Result = (U)Convert.ChangeType(sqlResult[keyCollumn], typeof(U));
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
                foreach (var pr in properties)
                {
                    if (pr.Name.Equals(keyCollumn, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    setQuery.AppendFormat("[{0}] =@{1},", pr.Name, pr.Name);
                }
                setQuery.Remove(setQuery.Length - 1, 1);
                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("UPDATE {0} SET {1} WHERE {2}=@{2}", tableName, setQuery.ToString(), keyCollumn);

                using var connection = new MySqlConnection(configuration["MySqlSettings:ConnectionString"]);
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = querybuilder.ToString();
                cmd.Prepare();
                foreach (var pr in properties)
                {
                    //if (pr.Name.Equals(keyCollumn, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    continue;
                    //}
                    cmd.Parameters.Add("@" + pr.Name, Helpers.Helpers.ConvertToMySqlDbType(pr.PropertyType).Value).Value = pr.GetValue(request);//map data type wise
                }
                cmd.CommandType = CommandType.Text;
                var sqlResult = await cmd.ExecuteNonQueryAsync();
                result = new BaseResult<bool>();
                if (sqlResult != 0)
                {
                    result.Result = true;

                }
                else
                {
                    result.Result = false;
                }
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
