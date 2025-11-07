using SDMS.Common.Infra.Attributes;
using SDMS.Common.Infra.Models;
using static SDMS.Common.Infra.Constants.Enums;


namespace SDMS.Models.ContentManagementModels
{
    [TableName("CMS_Banner")]
    public class Banner: Entity<int>
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
