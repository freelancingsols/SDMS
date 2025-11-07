using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.DL.FerretDB.Interface
{
    public interface IFerretDBConnection
    {
        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
    }
}

