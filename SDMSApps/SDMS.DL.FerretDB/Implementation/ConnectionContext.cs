using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SDMS.Common.Infra.Attributes;
using SDMS.DL.FerretDB.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SDMS.DL.FerretDB.Implementation
{
    public class ConnectionContext<T> : IConnectionContext<T>
    {
        public IMongoCollection<T> Collection { get; }
        private readonly IFerretDBConnection FerretDBConnection;
        
        public ConnectionContext(IFerretDBConnection ferretDBConnection)
        {
            FerretDBConnection = ferretDBConnection;
            var collectionAttribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
            Collection = FerretDBConnection.Database.GetCollection<T>(
                collectionAttribute != null ? collectionAttribute.Value : typeof(T).Name);
        }
    }
}

