using System;
using System.Collections.Generic;
using System.Text;
using static SDSM.Common.Infra.Constants.Enums;

namespace SDSM.Common.Infra.Models
{
    public class OrderByModel
    {
        public string Collumn { get; set; }
        public OrderByOperator Direction { get; set; }
    }
}
