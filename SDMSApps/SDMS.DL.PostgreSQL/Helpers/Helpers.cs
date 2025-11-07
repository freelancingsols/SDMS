using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.DL.PostgreSQL.Helpers
{
    public static class Helpers
    {
        public static NpgsqlDbType? ConvertToNpgsqlDbType(Type type)
        {
            NpgsqlDbType? returnType = null;
            if (type == typeof(byte))
            {
                returnType = NpgsqlDbType.Smallint;
            }
            else if (type == typeof(int))
            {
                returnType = NpgsqlDbType.Integer;
            }
            else if (type == typeof(long))
            {
                returnType = NpgsqlDbType.Bigint;
            }
            else if (type == typeof(float))
            {
                returnType = NpgsqlDbType.Real;
            }
            else if (type == typeof(double))
            {
                returnType = NpgsqlDbType.Double;
            }
            else if (type == typeof(string))
            {
                returnType = NpgsqlDbType.Varchar;
            }
            else if (type == typeof(DateTime))
            {
                returnType = NpgsqlDbType.Timestamp;
            }
            else if (type == typeof(Guid))
            {
                returnType = NpgsqlDbType.Uuid;
            }
            else if (type == typeof(decimal))
            {
                returnType = NpgsqlDbType.Numeric;
            }
            else if (type == typeof(bool))
            {
                returnType = NpgsqlDbType.Boolean;
            }
            else if (type == typeof(byte[]))
            {
                returnType = NpgsqlDbType.Bytea;
            }
            else
            {
                //unknown data type err 
            }
            return returnType;
        }
    }
}

