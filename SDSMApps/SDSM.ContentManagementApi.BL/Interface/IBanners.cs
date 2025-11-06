using SDSM.Common.Infra.Models;
using SDSM.Models.ContentManagementModels;
using SDSM.ViewModels.ContentManagementViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDSM.ContentManagementApi.BL.Interface
{
    public interface IBanners
    {
        Task<BaseResult<int>> Insert(BannerViewModel request);
        Task<BaseResult<bool>> Update(BannerViewModel request);
        Task<BaseResult<BannerViewModel>> GetById(int id);
    }
}
