using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using OOP_NoSQL.Domain.Interfaces;

namespace OOP_NoSQL.Infrastructure
{
    public class AuditLogDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class MongoAuditLogger : IAuditLogger
    {
        private readonly IMongoCollection<AuditLogDocument> _LogCollection;

        public MongoAuditLogger(string connectionString = "mongodb://localhost:27017", string databaseName = "BankingAuditDb")
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _LogCollection = database.GetCollection<AuditLogDocument>("AuditLogs");
        }

        public async Task LogActivityAsync(string userId, string action, object? details = null)
        {
            var log = new AuditLogDocument
            {
                UserId = userId,
                Action = action,
                Details = details?.ToString(),
                Timestamp = DateTime.UtcNow
            };
            await _LogCollection.InsertOneAsync(log);
        }

        public async Task LogErrorAsync(string action, string errorMessage)
        {
            var log = new AuditLogDocument
            {
                UserId = "SYSTEM",
                Action = $"Error_{action}",
                Details = errorMessage,
                Timestamp = DateTime.UtcNow
            };
            await _LogCollection.InsertOneAsync(log);
        }
    }
}
