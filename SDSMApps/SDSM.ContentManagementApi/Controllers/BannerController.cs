using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SDSM.ContentManagementApi.Interface;
using SDSM.ViewModels.ContentManagementViewModels;

namespace SDSM.ContentManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : Controller
    {
        private readonly IBanner _banners;

        public BannerController(IBanner _banners)
        {
            this._banners = _banners;
        }
        [HttpPost]
        public async Task<IActionResult> Insert([FromBody]BannerViewModel request)
        {
            var result = await _banners.Insert(request);
            if(result!=null && result.IsError && result.Exception!=null)
            {
                return new StatusCodeResult(500);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Update([FromBody]BannerViewModel request)
        {
            var result = await _banners.Update(request);
            if (result != null && result.IsError && result.Exception != null)
            {
                return new StatusCodeResult(500);
            }
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _banners.GetById(id);
            if (result != null && result.IsError && result.Exception != null)
            {
                return new StatusCodeResult(500);
            }
            return Ok(result);
        }

    }
       
}
