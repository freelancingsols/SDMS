using SDSM.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.DL.MySql.Interface
{
    interface ISqlDBOperationsEntity<T,U>: ISqlDBOperations<T> where T:Entity<U> where U :struct
                
    {
        BaseResult<T> Insert(T request);
        BaseResult<T> Update(T request);
        BaseResult<T> Delete(U value);
        BaseResult<T> Get(U value);

    }
}
