using MongoDB.Driver;
using SDMS.Common.Infra.Attributes;
using SDMS.Common.Infra.Models;
using SDMS.DL.MongoDB.Interface;
using SDMS.DL.MySql.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.MongoDB.Implementation
{
    public class NoSqlDBOperationsEntity<T, U> : NoSqlDBOperations<T>, INoSqlDBOperationsEntity<T, U> where T : Entity<U> where U : struct
    {
        public NoSqlDBOperationsEntity(IConnectionContext<T> context) : base(context)
        {
        }

        public async Task<BaseResult<bool>> Delete(U value)
        {
            BaseResult<bool> result=null;
            try
            {
                var filter = Builders<T>.Filter.Eq(x => x.Id, value);
                var deleteResult = await this.context.Collection.DeleteOneAsync(filter);
                if (deleteResult.IsAcknowledged)
                {
                    result = new BaseResult<bool>()
                    {
                        Result = deleteResult.IsAcknowledged
                    };
                    return result;
                }
                else 
                {
                    result = new BaseResult<bool>()
                    {
                        IsError = true,
                        Result = deleteResult.IsAcknowledged
                    };
                    return result;
                }
            }
            catch (Exception ex) 
            {
                result = new BaseResult<bool>()
                {
                    IsError = true,
                    Exception=ex
                };
                return result;
            }
        }

        public async Task<BaseResult<T>> Get(U value)
        {
            BaseResult<T> result = null;
            try
            {
                var filter = Builders<T>.Filter.Eq(x => x.Id, value);
                using (var getCursorResult = await this.context.Collection.FindAsync(filter)) 
                {
                    var getResult = getCursorResult.FirstOrDefault();
                    if (getResult!=null)
                    {
                        result = new BaseResult<T>()
                        {
                            Result = getResult
                        };
                        return result;
                    }
                    else
                    {
                        result = new BaseResult<T>()
                        {
                            IsError = true
                        };
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result = new BaseResult<T>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }

        public async Task<BaseResult<bool>> Insert(T request)
        {
            BaseResult<bool> result = null;
            try
            {
                var insertResultTask = this.context.Collection.InsertOneAsync(request);
                await insertResultTask;
                if (insertResultTask.IsCompleted)
                {
                    result = new BaseResult<bool>()
                    {
                        Result = insertResultTask.IsCompleted
                    };
                    return result;
                }
                else
                {
                    result = new BaseResult<bool>()
                    {
                        IsError = true,
                        Result = insertResultTask.IsCompleted
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new BaseResult<bool>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }

        public async Task<BaseResult<bool>> Update(T request)
        {
            BaseResult<bool> result = null;
            try
            {
                var filter = Builders<T>.Filter.Eq(x => x.Id, request.Id);
                var updateFilter = Builders<T>.Update.Set(x => x.Id, request.Id);
                var updateResult = await this.context.Collection.UpdateOneAsync(filter, updateFilter);
                if (updateResult.IsAcknowledged)
                {
                    result = new BaseResult<bool>()
                    {
                        Result = updateResult.IsAcknowledged
                    };
                    return result;
                }
                else
                {
                    result = new BaseResult<bool>()
                    {
                        IsError = true,
                        Result = updateResult.IsAcknowledged
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new BaseResult<bool>()
                {
                    IsError = true,
                    Exception = ex
                };
                return result;
            }
        }
    }
}
