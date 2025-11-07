using SDMS.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.FerretDB.Interface
{
    public interface IFerretDBOperations<T> where T : class
    {
        /// <summary>
        /// Get list of records by filter and order by
        /// </summary>
        /// <param name="filter">Func<T, bool> filter expression</param>
        /// <param name="orderBy">Order by configuration</param>
        /// <returns>List of entities</returns>
        Task<BaseResult<IList<T>>> GetListByFilter(Func<T, bool> filter, List<OrderByPredicateModel<T>> orderBy = null);
    }
}
