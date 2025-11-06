using SDSM.Common.Infra.Models;
using SDSM.ContentManagementApi.BL.Interface;
using SDSM.ContentManagementApi.BL.Mapper.Request;
using SDSM.ContentManagementApi.BL.Mapper.Response;
using SDSM.DL.MySql.Interface;
using SDSM.Models.ContentManagementModels;
using SDSM.ViewModels.ContentManagementViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDSM.ContentManagementApi.BL.Implementation
{
    public class Banners:IBanners
    {
        private readonly ISqlDBOperationsEntity<Banner,int> iBannerEntity;
        public Banners(ISqlDBOperationsEntity<Banner, int> iBannerEntity)
        {
            this.iBannerEntity = iBannerEntity;
        }
        /// <summary>
        /// Insert Banner 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResult<int>> Insert (BannerViewModel request)
        {
            var banner = BannerRequest.MapBannerRequest(request);
            return await iBannerEntity.Insert(banner);           
        }

        /// <summary>
        /// Update banner
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResult<bool>> Update (BannerViewModel request)
        {
            var banner = BannerRequest.MapBannerRequest(request);
            return await iBannerEntity.Update(banner);
        }
        /// <summary>
        /// Get banner by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BaseResult<BannerViewModel>> GetById(int id)
        {
            BaseResult<BannerViewModel> response = new BaseResult<BannerViewModel>(); 
            var banner= await iBannerEntity.Get(id);
            response.Result= BannerResponse.MapBannerResponse(banner.Result);
            return response;

        }
    }
}
