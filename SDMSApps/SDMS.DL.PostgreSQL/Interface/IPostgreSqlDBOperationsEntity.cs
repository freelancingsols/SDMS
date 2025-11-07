using SDMS.Common.Infra.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.PostgreSQL.Interface
{
    public interface IPostgreSqlDBOperationsEntity<T, U> : IPostgreSqlDBOperations<T> where T : Entity<U> where U : struct
    {
        Task<BaseResult<U>> Insert(T request);
        Task<BaseResult<bool>> Update(T request);
        Task<BaseResult<bool>> Delete(U value);
        Task<BaseResult<T>> Get(U value);
    }
}

