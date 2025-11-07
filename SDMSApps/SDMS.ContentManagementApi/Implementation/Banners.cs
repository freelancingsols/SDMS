using SDMS.Common.Infra.Models;
using SDMS.ContentManagementApi.BL.Interface;
using SDMS.ContentManagementApi.Interface;
using SDMS.ViewModels.ContentManagementViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDMS.ContentManagementApi.Implementation
{
    public class Banners:IBanner
    {
        private readonly IBanners bannerBl;
        public Banners(IBanners bannerBl)
        {
            this.bannerBl = bannerBl;
        }
        public async Task<BaseResult<int>> Insert(BannerViewModel request)
        {
            
            return await bannerBl.Insert(request);
        }

        /// <summary>
        /// Update banner
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResult<bool>> Update(BannerViewModel request)
        {        
            return await bannerBl.Update(request);
        }
        /// <summary>
        /// Get banner by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BaseResult<BannerViewModel>> GetById(int id)
        {
            return await bannerBl.GetById(id);
        }
    }
}
