using System;
using System.Collections.Generic;
using System.Text;
using static SDSM.Common.Infra.Constants.Enums;

namespace SDSM.Common.Infra.Models
{
    public class FilterModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public ConditionalOperator ConditionalOperator { get; set; }
        public LogicalOperator LogicalOperator { get; set; }
    }
}
