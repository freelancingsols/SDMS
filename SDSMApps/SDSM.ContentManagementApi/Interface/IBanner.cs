using SDSM.Common.Infra.Models;
using SDSM.ViewModels.ContentManagementViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDSM.ContentManagementApi.Interface
{
    public interface IBanner
    {
        Task<BaseResult<int>> Insert(BannerViewModel request);
        Task<BaseResult<bool>> Update(BannerViewModel request);
        Task<BaseResult<BannerViewModel>> GetById(int id);
    }
}
