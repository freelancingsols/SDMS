
using System;

namespace SDMS.Common.Infra.Models
{
    public class BaseModel
    {
       
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; } 

    }
}
