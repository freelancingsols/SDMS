using System;
using System.Collections.Generic;
using System.Text;
using static SDSM.Common.Infra.Constants.Enums;

namespace SDSM.ViewModels.ContentManagementViewModels
{
    public class BannerViewModel:BaseViewModel
    {
        public string Name { get; set; }
        public BannerSizeType Size { get; set; }
        public string ImageId { get; set; }
        public string ImageURL { get; set; }
        public string BannerRedirectingURL { get; set; }
        public int? CategoryId { get; set; }
        public int? OutletId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
