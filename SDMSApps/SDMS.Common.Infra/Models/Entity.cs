using SDMS.Common.Infra.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.Common.Infra.Models
{
    public class Entity<T> :BaseModel where T : struct
    {
        [Key]
        public virtual T Id { get; set; }
    }
}
