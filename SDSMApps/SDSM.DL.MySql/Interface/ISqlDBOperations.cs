using SDSM.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.DL.MySql.Interface
{
    public interface ISqlDBOperations<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>IDictionary<key,value,comparison operator> filter 
        /// <param name="orderBy"></param>
        /// <returns></returns>
        BaseResult<T> GetListByFilter(IList<FilterModel> filter=null, IList<OrderByModel> orderBy =null);
    }
}
