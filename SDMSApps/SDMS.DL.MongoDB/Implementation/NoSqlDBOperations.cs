using MongoDB.Driver;
using SDMS.Common.Infra.Models;
using SDMS.DL.MongoDB.Interface;
using SDMS.DL.MySql.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SDMS.Common.Infra.Constants.Enums;

namespace SDMS.DL.MongoDB.Implementation
{
    public class NoSqlDBOperations<T> : INoSqlDBOperations<T> where T : class
    {
        public readonly IConnectionContext<T> context;
        public NoSqlDBOperations(IConnectionContext<T> context)
        {
            this.context = context;
        }
        public async Task<BaseResult<IList<T>>> GetListByFilter(Func<T, bool> filter, List<OrderByPredicateModel<T>> orderBy = null)
        {
            //todo:https://docs.mongodb.com/manual/tutorial/query-documents/
            BaseResult<IList<T>> result=null;
            var dataResult = await Task.FromResult(context.Collection.AsQueryable().Where(filter));
            //var dataResult = await Task.Run(
            //    () => 
            //    {
            //        return context.Collection.AsQueryable().Where(filter); 
            //    }
            //    );
            //.Select(x=>x).OrderBy(y=>y).ThenBy(z=>z);//IOrderedEnumerable
            if (orderBy == null) 
            {
                result = new BaseResult<IList<T>>()
                {
                    Result = dataResult.ToList()
                };
                return result;
            }
            var isFirstOrderByProcessed = false;
            foreach (var item in orderBy) 
            {
                if (!isFirstOrderByProcessed)
                {
                    if (item.Direction == OrderByOperator.Ascending)
                    {
                        dataResult = dataResult.OrderBy(item.Collumn);
                    }
                    else if (item.Direction == OrderByOperator.Descending) 
                    {
                        dataResult = dataResult.OrderByDescending(item.Collumn);
                    }
                    isFirstOrderByProcessed = true;
                }
                else 
                {
                    if (item.Direction == OrderByOperator.Ascending)
                    {
                        dataResult = ((IOrderedEnumerable<T>)dataResult).ThenBy(item.Collumn);
                    }
                    else if (item.Direction == OrderByOperator.Descending)
                    {
                        dataResult = ((IOrderedEnumerable<T>)dataResult).ThenBy(item.Collumn);
                    }
                }
            }
            result = new BaseResult<IList<T>>()
            {
                Result = dataResult.ToList()
            };
            return result;
            #region poc
            //Builders<T>.Filter.Where(x =>
            //{
            //    var properties = typeof(T).GetProperties(BindingFlags.Public);

            //    foreach (var filter in filters)
            //    {
            //        var pr = properties.FirstOrDefault(x => x.Name.Equals(filter.Key, StringComparison.OrdinalIgnoreCase));
            //        if (pr != null)
            //        {
            //            if (filter.ConditionalOperator == "=")
            //            {
            //                filter.ExpressionOutput = pr.GetValue(x).ToString().Equals(filter.Value, StringComparison.OrdinalIgnoreCase);
            //            }
            //            else if (filter.ConditionalOperator == "!=")
            //            {
            //                filter.ExpressionOutput = !pr.GetValue(x).ToString().Equals(filter.Value, StringComparison.OrdinalIgnoreCase);
            //            }
            //            else if (filter.ConditionalOperator == ">")
            //            {
            //                  //convert filter.value to pr type and do comparison 
            //            }
            //            else if (filter.ConditionalOperator == "<")
            //            {
            //            }
            //            else if (filter.ConditionalOperator == "=<")
            //            {
            //            }
            //            else if (filter.ConditionalOperator == "=<")
            //            {
            //            }
            //            else 
            //            {
            //                //err
            //            }
            //        }
            //        else
            //        {
            //            //err
            //        }
            //    }
            //});
            //this.context.Collection.;
            #endregion
            throw new NotImplementedException();
        }
    }
}
