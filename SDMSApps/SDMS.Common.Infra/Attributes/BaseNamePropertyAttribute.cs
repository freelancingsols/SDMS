using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.Common.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BaseNamePropertyAttribute : BaseNameAttribute
    {
        public BaseNamePropertyAttribute(string value):base(value)
        {
        }
    }
}
