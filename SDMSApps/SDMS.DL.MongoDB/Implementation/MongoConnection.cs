using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SDMS.DL.MongoDB.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDMS.DL.MongoDB.Implementation
{
    public class MongoConnection : IMongoConnection
    {
        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
        public MongoConnection(IConfiguration configuration)
        {
            Client=new MongoClient(configuration["MongoSettings:ConnectionString"]);
            Database = Client.GetDatabase(configuration["MongoSettings:DataBaseName"]);
        }
    }
}
