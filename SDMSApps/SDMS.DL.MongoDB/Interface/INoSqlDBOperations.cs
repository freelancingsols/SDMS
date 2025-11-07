using SDMS.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.MySql.Interface
{
    public interface INoSqlDBOperations<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>IDictionary<key,value,comparison operator> filter 
        /// <param name="orderBy"></param>
        /// <returns></returns>
        Task<BaseResult<IList<T>>> GetListByFilter(Func<T, bool> filter, List<OrderByPredicateModel<T>> orderBy = null);
    }
}
