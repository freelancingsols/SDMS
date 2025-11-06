using SDSM.Common.Infra.Models;
using SDSM.DL.MySql.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SDSM.Common.Infra.Attributes;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;
using static SDSM.Common.Infra.Constants.Enums;
using SDSM.Common.Infra.Constants;
using System.Text.RegularExpressions;

namespace SDSM.DL.MySql.Implementation
{
    public class SqlDBOperations<T> : ISqlDBOperations<T> where T : class
    {
        public readonly IConfiguration configuration;
        public readonly string tableName;
        public readonly string keyCollumn;
        public readonly PropertyInfo[] properties;
        public SqlDBOperations(IConfiguration configuration)
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
                    collumnQuery.AppendFormat("[{0}],", pr.Name);
                }
                if (filter != null && filter.Any())
                {
                    foreach (var fr in filter) 
                    {
                        whereQueryBuilder.AppendFormat("[{0}]", fr.Key);
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
                            orderByQuery.AppendFormat("[{0}] ASC ,", or.Collumn);
                        }
                        else if (or.Direction == OrderByOperator.Descending)
                        {
                            orderByQuery.AppendFormat("[{0}] DESC ,", or.Collumn);
                        }
                        else 
                        {
                            //err
                        }
                        
                    }
                }
                collumnQuery.Remove(collumnQuery.Length - 1, 1);
                orderByQuery.Remove(orderByQuery.Length - 1, 1);
                string whereQuery = null;
                if (whereQuery.EndsWith(Constants.MySqlLogicalOperator.And))
                {
                    whereQuery = whereQueryBuilder.ToString().TrimEnd(Constants.MySqlLogicalOperator.And.ToCharArray());
                }
                else if (whereQuery.EndsWith(Constants.MySqlLogicalOperator.Or))
                {
                    whereQuery = whereQueryBuilder.ToString().TrimEnd(Constants.MySqlLogicalOperator.Or.ToCharArray());
                }
                else 
                {
                    //err
                }
                var querybuilder = new StringBuilder();
                querybuilder.AppendFormat("SELECT {0} FROM {1} WHERE {2} ORDER BY {3};", collumnQuery.ToString(), tableName, whereQuery, orderByQuery.ToString());
                using var connection = new MySqlConnection(configuration["MySqlSettings:ConnectionString"]);
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = querybuilder.ToString();
                cmd.Prepare();
                if (filter != null && filter.Any())
                {
                    foreach (var fr in filter)
                    {
                        var pr = properties.First(x => x.Name.Equals(fr.Key, StringComparison.OrdinalIgnoreCase));
                        cmd.Parameters.Add("@" + fr.Key, Helpers.Helpers.ConvertToMySqlDbType(pr.PropertyType).Value).Value =Convert.ChangeType(fr.Value, pr.PropertyType);
                    }
                }
                cmd.CommandType = CommandType.Text;
                var sqlResult = await cmd.ExecuteReaderAsync();
                result = new BaseResult<IList<T>>() 
                {
                    Result=new List<T>()
                };
                while (sqlResult.Read())
                {
                    var tuple = (T)Activator.CreateInstance(typeof(T));
                    //TypeDescriptor.GetConverter(typeof(U)).ConvertFrom(sqlResult[keyCollumn]);
                    foreach (var pr in properties)
                    {
                        pr.SetValue(tuple, Convert.ChangeType(sqlResult[pr.Name], pr.PropertyType));
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
