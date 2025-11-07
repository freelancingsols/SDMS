using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SDMS.DL.FerretDB.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.DL.FerretDB.Implementation
{
    public class FerretDBConnection : IFerretDBConnection
    {
        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
        
        public FerretDBConnection(IConfiguration configuration)
        {
            // FerretDB is MongoDB-compatible, so we use the same MongoDB driver
            // Connection string format is the same as MongoDB
            Client = new MongoClient(configuration["FerretDBSettings:ConnectionString"]);
            Database = Client.GetDatabase(configuration["FerretDBSettings:DataBaseName"]);
        }
    }
}
