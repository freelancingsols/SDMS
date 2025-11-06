using MySql.Data.MySqlClient;
using SDSM.Common.Infra.Attributes;
using SDSM.Common.Infra.Models;
using SDSM.DL.MySql.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SDSM.DL.MySql.Implementation
{
    public class SqlDBOperationsEntity<T, U> : SqlDBOperations<T>, ISqlDBOperationsEntity<T, U> where T : Entity<U> where U : struct
    {
        private readonly string tableName;
        private readonly string keyCollumn;
        public SqlDBOperationsEntity():base()
        {
            var type = typeof(T);
            tableName = type.IsDefined(typeof(TableNameAttribute)) ? type.GetCustomAttribute<TableNameAttribute>().Value : type.Name;
            var properties = typeof(T).GetProperties(BindingFlags.Public);
            var keyProperty = properties.FirstOrDefault(x => x.IsDefined(typeof(KeyAttribute)));
            if (keyProperty != null)
            {
                keyCollumn = keyProperty.Name;
            }
            else 
            {
                keyCollumn = "Id";
            }

        }
        public BaseResult<T> Delete(U value)
        {
            throw new NotImplementedException();
        }

        public BaseResult<T> Get(U value)
        {
            throw new NotImplementedException();
        }

        public BaseResult<T> Insert(T request)
        {
            var type = typeof(T);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var collumnQuery = new StringBuilder();
            var valuesQuery = new StringBuilder();
            foreach (var pr in properties)
            {
                if (pr.Name.Equals(keyCollumn,StringComparison.OrdinalIgnoreCase)) 
                {
                    continue;
                }
                collumnQuery.AppendFormat("[{0}],", pr.Name);
                valuesQuery.AppendFormat("[{0}] =@{1},", pr.Name, pr.Name);
            }
            collumnQuery.Remove(collumnQuery.Length-1, 1);
            valuesQuery.Remove(valuesQuery.Length-1, 1);

            var querybuilder = new StringBuilder();
            querybuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2}); SELECT  LAST_INSERT_ID();", tableName, collumnQuery.ToString(), valuesQuery.ToString());

            using (var connection = new MySqlConnection()) 
            {
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
                    cmd.Parameters.Add("@"+pr.Name, MySqlDbType.Text).Value= pr.GetValue(request);
                }

                var result = cmd.BeginExecuteReader();

            }
            throw new NotImplementedException();
        }

        public BaseResult<T> Update(T request)
        {
            throw new NotImplementedException();
        }
    }
}
