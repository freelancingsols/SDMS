using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.Common.Infra.Models
{
    public class BaseResult<T>
    {
        public T Result { get; set; }
        public bool IsError { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }
}
