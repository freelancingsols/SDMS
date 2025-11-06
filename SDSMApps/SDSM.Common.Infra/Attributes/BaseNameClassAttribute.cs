using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.Common.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BaseNameClassAttribute : BaseNameAttribute
    {
        public BaseNameClassAttribute(string value):base(value)
        {
        }
    }
}
