using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.Common.Infra.Constants
{
    public static class Constants
    {
        public struct ConditionalOperator 
        {
            public const string Equal = "==";
            public const string NotEqueal = "!=";
            public const string LessThan = "<";
            public const string GreaterThan = ">";
            public const string LessThanEqualTo = "<=";
            public const string GreaterThanEqualTo = ">=";
        }
        public struct LogicalOperator
        {
            public const string And = "&&";
            public const string Or = "||";
        }
        public struct MySqlConditionalOperator
        {
            public const string Equal = "=";
            public const string NotEqueal = "<>";
            public const string LessThan = "<";
            public const string GreaterThan = ">";
            public const string LessThanEqualTo = "<=";
            public const string GreaterThanEqualTo = ">=";
        }
        public struct MySqlLogicalOperator
        {
            public const string And = "and";
            public const string Or = "or";
        }
    }
}
