using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.DL.MySql.Helpers
{
    public static class Helpers
    {
        public static MySqlDbType? ConvertToMySqlDbType(Type type) 
        {
            MySqlDbType? returnType=null; 
            if (type == typeof(byte))
            {
                returnType = MySqlDbType.Byte;
            }
            else if (type == typeof(int))
            {
                returnType = MySqlDbType.Int32;
            }
            else if (type == typeof(long))
            {
                returnType = MySqlDbType.Int64;
            }
            else if (type == typeof(float))
            {
                returnType = MySqlDbType.Float;
            }
            else if (type == typeof(double))
            {
                returnType = MySqlDbType.Double;
            }
            else if (type == typeof(string))
            {
                returnType = MySqlDbType.VarChar;
            }
            else if (type == typeof(DateTime))
            {
                returnType = MySqlDbType.DateTime;
            }
            else if (type == typeof(Guid))
            {
                returnType = MySqlDbType.Guid;
            }
            else if (type == typeof(decimal))
            {
                returnType = MySqlDbType.Decimal;
            }
            else 
            {
                //unknown data type err 
            }
            return returnType;
        }
    }
}
