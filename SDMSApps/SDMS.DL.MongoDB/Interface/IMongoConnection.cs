using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.DL.MongoDB.Interface
{
    public interface IMongoConnection
    {
        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
    }
}
