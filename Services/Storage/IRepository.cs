using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace StravaDiscordBot.Services.Storage
{
    public interface IRepository<T> where T : TableEntity
    {
        Task<List<T>> GetForPartition(string partitionKey);
        Task<T> GetById(string partitionKey, string rowKey);
        Task Save(T entity);
        Task Remove(T entity);
    }
}
