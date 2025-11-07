using MongoDB.Driver;
using SDMS.Common.Infra.Attributes;
using SDMS.Common.Infra.Models;
using SDMS.DL.FerretDB.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDMS.DL.FerretDB.Implementation
{
    public class FerretDBOperationsEntity<T, U> : FerretDBOperations<T>, IFerretDBOperationsEntity<T, U> where T : Entity<U> where U : struct
    {
        public FerretDBOperationsEntity(IConnectionContext<T> context) : base(context)
        {
        }

        public async Task<BaseResult<bool>> Delete(U value)
        {
            BaseResult<bool> result = null;
            try
            {
                var filter = Builders<T>.Filter.Eq(x => x.Id, value);
                var deleteResult = await this.context.Collection.DeleteOneAsync(filter);
                
                if (deleteResult.IsAcknowledged)
                {
                    result = new BaseResult<bool>()
                    {
                        Result = deleteResult.DeletedCount > 0
                    };
                    return result;
                }
                else
                {
                    result = new BaseResult<bool>()
                    {
                        IsError = true,
                        Result = false
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

        public async Task<BaseResult<T>> Get(U value)
        {
            BaseResult<T> result = null;
            try
            {
                var filter = Builders<T>.Filter.Eq(x => x.Id, value);
                using (var getCursorResult = await this.context.Collection.FindAsync(filter))
                {
                    var getResult = await getCursorResult.FirstOrDefaultAsync();
                    if (getResult != null)
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
                await this.context.Collection.InsertOneAsync(request);
                result = new BaseResult<bool>()
                {
                    Result = true
                };
                return result;
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
                
                // Use ReplaceOneAsync to replace the entire document
                // This is simpler and works well with FerretDB
                var updateResult = await this.context.Collection.ReplaceOneAsync(filter, request);
                
                if (updateResult.IsAcknowledged)
                {
                    result = new BaseResult<bool>()
                    {
                        Result = updateResult.ModifiedCount > 0 || updateResult.MatchedCount > 0
                    };
                    return result;
                }
                else
                {
                    result = new BaseResult<bool>()
                    {
                        IsError = true,
                        Result = false
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

