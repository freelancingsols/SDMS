using SDSM.Common.Infra.Models;
using SDSM.DL.MySql.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.DL.MySql.Implementation
{
    public class SqlDBOperations<T> : ISqlDBOperations<T> where T : class
    {
        public BaseResult<T> GetListByFilter(IList<FilterModel> filter = null, IList<OrderByModel> orderBy = null)
        {
            throw new NotImplementedException();
        }
    }
}
