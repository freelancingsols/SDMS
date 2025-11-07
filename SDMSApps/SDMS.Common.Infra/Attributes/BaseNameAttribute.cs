using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.Common.Infra.Attributes
{
    public class BaseNameAttribute : Attribute
    {
        public string Value { get;}
        public BaseNameAttribute(string value)
        {
            Value = value;
        }
    }
    
}
