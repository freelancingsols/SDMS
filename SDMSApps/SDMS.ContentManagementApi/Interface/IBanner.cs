using SDMS.Common.Infra.Models;
using SDMS.ViewModels.ContentManagementViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDMS.ContentManagementApi.Interface
{
    public interface IBanner
    {
        Task<BaseResult<int>> Insert(BannerViewModel request);
        Task<BaseResult<bool>> Update(BannerViewModel request);
        Task<BaseResult<BannerViewModel>> GetById(int id);
    }
}
