using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.Common.Infra.Attributes
{
    public class TableNameAttribute: BaseNameClassAttribute
    {
        public TableNameAttribute(string value):base(value)
        {
        }
    }
}
