using SDMS.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.PostgreSQL.Interface
{
    public interface IPostgreSqlDBOperations<T> where T : class
    {
        /// <summary>
        /// Get list of records by filter and order by
        /// </summary>
        /// <param name="filter">IDictionary<key,value,comparison operator> filter</param>
        /// <param name="orderBy">Order by configuration</param>
        /// <returns>List of entities</returns>
        Task<BaseResult<IList<T>>> GetListByFilter(IList<FilterModel> filter = null, IList<OrderByModel> orderBy = null);
    }
}

