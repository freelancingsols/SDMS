using SDMS.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.MySql.Interface
{
    interface INoSqlDBOperationsEntity<T,U>: INoSqlDBOperations<T> where T:Entity<U> where U :struct
                
    {
        Task<BaseResult<bool>> Insert(T request);
        Task<BaseResult<bool>> Update(T request);
        Task<BaseResult<bool>> Delete(U value);
        Task<BaseResult<T>> Get(U value);

    }
}
