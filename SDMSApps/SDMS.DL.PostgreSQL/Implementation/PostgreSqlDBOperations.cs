using SDMS.Common.Infra.Models;
using SDMS.DL.PostgreSQL.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SDMS.Common.Infra.Attributes;
using System.Linq;
using Npgsql;
using System.Data;
using static SDMS.Common.Infra.Constants.Enums;
using SDMS.Common.Infra.Constants;
using SDMS.DL.PostgreSQL.Helpers;

namespace SDMS.DL.PostgreSQL.Implementation
{
    public class PostgreSqlDBOperations<T> : IPostgreSqlDBOperations<T> where T : class
    {
        public readonly IConfiguration configuration;
        public readonly string tableName;
        public readonly string keyCollumn;
        public readonly PropertyInfo[] properties;
        
        public PostgreSqlDBOperations(IConfiguration configuration)
        {
            var type = typeof(T);
            tableName = type.IsDefined(typeof(TableNameAttribute)) ? type.GetCustomAttribute<TableNameAttribute>().Value : type.Name;
            properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var keyProperty = properties.FirstOrDefault(x => x.IsDefined(typeof(KeyAttribute)));
            if (keyProperty != null)
            {
                keyCollumn = keyProperty.Name;
            }
            else
            {
                keyCollumn = "Id";
            }
            this.configuration = configuration;
        }
        
        public async Task<BaseResult<IList<T>>> GetListByFilter(IList<FilterModel> filter = null, IList<OrderByModel> orderBy = null)
        {
            BaseResult<IList<T>> result;
            try
            {
                var collumnQuery = new StringBuilder();
                var whereQueryBuilder = new StringBuilder();
                var orderByQuery = new StringBuilder();
                
                foreach (var pr in properties)
                {
                    collumnQuery.AppendFormat("\"{0}\",", pr.Name);
                }
                
                if (filter != null && filter.Any())
                {
                    foreach (var fr in filter)
                    {
                        whereQueryBuilder.AppendFormat("\"{0}\"", fr.Key);
                        if (fr.ConditionalOperator == ConditionalOperator.Equal)
                        {
                            whereQueryBuilder.Append(Constants.MySqlConditionalOperator.Equal);
                        }
                        else if (fr.ConditionalOperator == ConditionalOperator.NotEqueal)
                        {
                            whereQueryBuilder.Append(Constants.MySqlConditionalOperator.NotEqueal);
                        }
                        else if (fr.ConditionalOperator == ConditionalOperator.GreaterThan)
                        {
                            whereQueryBuilder.Append(Constants.MySqlConditionalOperator.GreaterThan);
                        }
                        else if (fr.ConditionalOperator == ConditionalOperator.GreaterThanEqualTo)
                        {
                            whereQueryBuilder.Append(Constants.MySqlConditionalOperator.GreaterThanEqualTo);
                        }
                        else if (fr.ConditionalOperator == ConditionalOperator.LessThan)
                        {
                            whereQueryBuilder.Append(Constants.MySqlConditionalOperator.LessThan);
                        }
                        else if (fr.ConditionalOperator == ConditionalOperator.LessThanEqualTo)
                        {
                            whereQueryBuilder.Append(Constants.MySqlConditionalOperator.LessThanEqualTo);
                        }
                        else
                        {
                            //err
                        }
                        whereQueryBuilder.AppendFormat("@{0}", fr.Key);
                        whereQueryBuilder.Append(" ");
                        if (fr.LogicalOperator == LogicalOperator.And)
                        {
                            whereQueryBuilder.Append(Constants.MySqlLogicalOperator.And);
                        }
                        else if (fr.LogicalOperator == LogicalOperator.Or)
                        {
                            whereQueryBuilder.Append(Constants.MySqlLogicalOperator.Or);
                        }
                        else
                        {
                            //err
                        }
                        whereQueryBuilder.Append(" ");
                    }
                }
                
                if (orderBy != null && orderBy.Any())
                {
                    foreach (var or in orderBy)
                    {
                        if (or.Direction == OrderByOperator.Ascending)
                        {
                            orderByQuery.AppendFormat("\"{0}\" ASC ,", or.Collumn);
                        }
                        else if (or.Direction == OrderByOperator.Descending)
                        {
                            orderByQuery.AppendFormat("\"{0}\" DESC ,", or.Collumn);
                        }
                        else
                        {
                            //err
                        }
                    }
                }
                
                if (collumnQuery.Length > 0)
                {
                    collumnQuery.Remove(collumnQuery.Length - 1, 1);
                }
                
                string whereQuery = null;
                if (whereQueryBuilder.Length > 0)
                {
                    var whereStr = whereQueryBuilder.ToString().TrimEnd();
                    if (whereStr.EndsWith(Constants.MySqlLogicalOperator.And, StringComparison.OrdinalIgnoreCase))
                    {
                        whereQuery = whereStr.Substring(0, whereStr.Length - Constants.MySqlLogicalOperator.And.Length).TrimEnd();
                    }
                    else if (whereStr.EndsWith(Constants.MySqlLogicalOperator.Or, StringComparison.OrdinalIgnoreCase))
                    {
                        whereQuery = whereStr.Substring(0, whereStr.Length - Constants.MySqlLogicalOperator.Or.Length).TrimEnd();
                    }
                    else
                    {
                        whereQuery = whereStr;
                    }
                }
                
                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("SELECT {0} FROM \"{1}\"", collumnQuery.ToString(), tableName);
                
                if (!string.IsNullOrEmpty(whereQuery))
                {
                    querybuilder.AppendFormat(" WHERE {0}", whereQuery);
                }
                
                if (orderByQuery.Length > 0)
                {
                    orderByQuery.Remove(orderByQuery.Length - 1, 1);
                    querybuilder.AppendFormat(" ORDER BY {0}", orderByQuery.ToString());
                }
                
                querybuilder.Append(";");
                
                using var connection = new NpgsqlConnection(configuration["PostgreSqlSettings:ConnectionString"]);
                await connection.OpenAsync();
                using var cmd = new NpgsqlCommand(querybuilder.ToString(), connection);
                
                if (filter != null && filter.Any())
                {
                    foreach (var fr in filter)
                    {
                        var pr = properties.First(x => x.Name.Equals(fr.Key, StringComparison.OrdinalIgnoreCase));
                        var dbType = SDMS.DL.PostgreSQL.Helpers.Helpers.ConvertToNpgsqlDbType(pr.PropertyType);
                        if (dbType.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@" + fr.Key, dbType.Value, Convert.ChangeType(fr.Value, pr.PropertyType) ?? DBNull.Value);
                        }
                    }
                }
                
                using var sqlResult = await cmd.ExecuteReaderAsync();
                result = new BaseResult<IList<T>>()
                {
                    Result = new List<T>()
                };
                
                while (await sqlResult.ReadAsync())
                {
                    var tuple = (T)Activator.CreateInstance(typeof(T));
                    foreach (var pr in properties)
                    {
                        var value = sqlResult[pr.Name];
                        if (value != DBNull.Value)
                        {
                            pr.SetValue(tuple, Convert.ChangeType(value, pr.PropertyType));
                        }
                    }
                    result.Result.Add(tuple);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                result = new BaseResult<IList<T>>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }
    }
}

