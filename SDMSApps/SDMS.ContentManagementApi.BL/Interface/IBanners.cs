using SDMS.Common.Infra.Models;
using SDMS.Models.ContentManagementModels;
using SDMS.ViewModels.ContentManagementViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.ContentManagementApi.BL.Interface
{
    public interface IBanners
    {
        Task<BaseResult<int>> Insert(BannerViewModel request);
        Task<BaseResult<bool>> Update(BannerViewModel request);
        Task<BaseResult<BannerViewModel>> GetById(int id);
    }
}
