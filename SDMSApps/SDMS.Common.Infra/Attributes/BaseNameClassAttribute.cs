using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.Common.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BaseNameClassAttribute : BaseNameAttribute
    {
        public BaseNameClassAttribute(string value):base(value)
        {
        }
    }
}
