using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDSM.DL.MongoDB.Interface
{
    public interface IConnectionContext<T>
    {
        public IMongoCollection<T> Collection { get;}
    }
}
