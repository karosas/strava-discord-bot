using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using StravaDiscordBot.Models;

namespace StravaDiscordBot.Services.Storage
{
    public class LeaderboardParticipantRepository : IRepository<LeaderboardParticipant>
    {
        private readonly AppOptions _options;
        private CloudTable _table;

        public LeaderboardParticipantRepository(AppOptions options)
        {
            _options = options;
        }

        private async Task<CloudTable> CreateTable()
        {
            string storageConnectionString = _options.StorageConnectionString;

            // Retrieve storage account information from connection string.
            var storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            var table = tableClient.GetTableReference("leaderboard");
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", "leaderboard");
            }
            else
            {
                Console.WriteLine("Table {0} already exists", "leaderboard");
            }

            Console.WriteLine();
            return table;
        }

        private CloudStorageAccount CreateStorageAccountFromConnectionString(string connectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

       

        public async Task<LeaderboardParticipant> GetById(string partitionKey, string rowKey)
        {
            if (_table == null)
                _table = await CreateTable();

            try
            {
                var retrieveOperation = TableOperation.Retrieve<LeaderboardParticipant>(partitionKey, rowKey);
                var result = await _table.ExecuteAsync(retrieveOperation);
                var entry = result.Result as LeaderboardParticipant;

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }

                return entry;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public async Task<List<LeaderboardParticipant>> GetForPartition(string partitionKey)
        {
            if (_table == null)
                _table = await CreateTable();

            try
            {
                var query = new TableQuery<LeaderboardParticipant>()
                    .Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                    );

                var result = _table.ExecuteQuery<LeaderboardParticipant>(query);
                return result.ToList();
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public async Task Remove(LeaderboardParticipant entity)
        {
            if (_table == null)
                _table = await CreateTable();

            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                var deleteOperation = TableOperation.Delete(entity);
                var result = await _table.ExecuteAsync(deleteOperation);

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure CosmoS DB 
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of Delete Operation: " + result.RequestCharge);
                }

            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public async Task Save(LeaderboardParticipant entity)
        {
            if (_table == null)
                _table = await CreateTable();
            try
            {
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}