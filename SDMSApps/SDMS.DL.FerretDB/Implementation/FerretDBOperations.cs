using MongoDB.Driver;
using SDMS.Common.Infra.Models;
using SDMS.DL.FerretDB.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SDMS.Common.Infra.Constants.Enums;

namespace SDMS.DL.FerretDB.Implementation
{
    public class FerretDBOperations<T> : IFerretDBOperations<T> where T : class
    {
        public readonly IConnectionContext<T> context;
        
        public FerretDBOperations(IConnectionContext<T> context)
        {
            this.context = context;
        }
        
        public async Task<BaseResult<IList<T>>> GetListByFilter(Func<T, bool> filter, List<OrderByPredicateModel<T>> orderBy = null)
        {
            BaseResult<IList<T>> result = null;
            try
            {
                var dataResult = await Task.FromResult(context.Collection.AsQueryable().Where(filter));
                
                if (orderBy == null)
                {
                    result = new BaseResult<IList<T>>()
                    {
                        Result = dataResult.ToList()
                    };
                    return result;
                }
                
                var isFirstOrderByProcessed = false;
                IOrderedEnumerable<T> orderedResult = null;
                
                foreach (var item in orderBy)
                {
                    if (!isFirstOrderByProcessed)
                    {
                        if (item.Direction == OrderByOperator.Ascending)
                        {
                            orderedResult = dataResult.OrderBy(item.Collumn);
                        }
                        else if (item.Direction == OrderByOperator.Descending)
                        {
                            orderedResult = dataResult.OrderByDescending(item.Collumn);
                        }
                        isFirstOrderByProcessed = true;
                    }
                    else
                    {
                        if (item.Direction == OrderByOperator.Ascending)
                        {
                            orderedResult = orderedResult.ThenBy(item.Collumn);
                        }
                        else if (item.Direction == OrderByOperator.Descending)
                        {
                            orderedResult = orderedResult.ThenByDescending(item.Collumn);
                        }
                    }
                }
                
                result = new BaseResult<IList<T>>()
                {
                    Result = orderedResult != null ? orderedResult.ToList() : dataResult.ToList()
                };
                return result;
            }
            catch (Exception ex)
            {
                result = new BaseResult<IList<T>>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }
    }
}

