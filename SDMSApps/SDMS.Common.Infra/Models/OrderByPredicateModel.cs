using System;
using System.Collections.Generic;
using System.Text;
using static SDMS.Common.Infra.Constants.Enums;

namespace SDMS.Common.Infra.Models
{
    public class OrderByPredicateModel<T>
    {
        public Func<T,T> Collumn { get; set; }
        public OrderByOperator Direction { get; set; }
    }
}
