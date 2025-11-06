using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.Common.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BaseNameMethodAttribute : BaseNameAttribute
    {
        public BaseNameMethodAttribute(string value):base(value)
        {
        }
    }
}
