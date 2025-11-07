using SDMS.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.MySql.Interface
{
    public interface ISqlDBOperations<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>IDictionary<key,value,comparison operator> filter 
        /// <param name="orderBy"></param>
        /// <returns></returns>
        Task<BaseResult<IList<T>>> GetListByFilter(IList<FilterModel> filter=null, IList<OrderByModel> orderBy =null);
    }
}
