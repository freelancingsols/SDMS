using SDMS.Models.ContentManagementModels;
using SDMS.ViewModels.ContentManagementViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.ContentManagementApi.BL.Mapper.Response
{
    public static class BannerResponse
    {
        public static BannerViewModel MapBannerResponse(Banner request)
        {
            BannerViewModel banner = new BannerViewModel
            {
                Name = request.Name,
                ImageId = request.ImageId,
                BannerRedirectingURL = request.BannerRedirectingURL,
                CategoryId = request.CategoryId,
                CreatedBy = request.CreatedBy,
                CreatedDate = request.CreatedDate,
                ImageURL = request.ImageURL,
                IsActive = request.IsActive,
                IsDeleted = request.IsDeleted,
                OutletId = request.OutletId,
                Size = request.Size,
                UpdatedBy = request.UpdatedBy,
                UpdatedDate = request.UpdatedDate

            };
            return banner;
        }
    }
}
