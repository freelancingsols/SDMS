using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.ViewModels
{
    public class BaseViewModel
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
