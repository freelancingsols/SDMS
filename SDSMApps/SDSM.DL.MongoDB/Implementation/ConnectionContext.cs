using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SDSM.Common.Infra.Attributes;
using SDSM.DL.MongoDB.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SDSM.DL.MongoDB.Implementation
{
    public class ConnectionContext<T> : IConnectionContext<T>
    {
        public IMongoCollection<T> Collection { get; }
        private readonly IMongoConnection MongoConnection;
        public ConnectionContext(IMongoConnection mongoConnection)
        {
            MongoConnection = mongoConnection;
            var collectionAttribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
            Collection = MongoConnection.Database.GetCollection<T>(collectionAttribute != null? collectionAttribute.Value: typeof(T).Name); ;
        }
    }
}
