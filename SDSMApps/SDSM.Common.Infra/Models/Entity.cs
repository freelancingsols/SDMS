using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.Common.Infra.Models
{
    public class Entity<T> where T : struct
    {
        public virtual T Id { get; set; }
    }
}
