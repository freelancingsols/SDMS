using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.Common.Infra.Constants
{
    public static class Enums
    {
        public enum ConditionalOperator
        {
            Equal,
            NotEqueal,
            LessThan,
            GreaterThan,
            LessThanEqualTo,
            GreaterThanEqualTo,
        }
        public enum LogicalOperator
        {
            And,
            Or
        }
        public enum OrderByOperator
        {
            Ascending,
            Descending
        }

        public enum BannerSizeType
        {
            Small,
            Large
        }
    }
}
