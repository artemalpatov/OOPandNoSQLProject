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
        private readonly IMongoCollection<AuditLogDocument> _loginLogs;
        private readonly IMongoCollection<AuditLogDocument> _transactionLogs;
        private readonly IMongoCollection<AuditLogDocument> _monthlyProcessesLogs;
        private readonly IMongoCollection<AuditLogDocument> _errorLogs;

        public MongoAuditLogger(string connectionString = "mongodb://localhost:27017", string databaseName = "BankingAuditDb")
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _loginLogs = database.GetCollection<AuditLogDocument>("LoginLogs");
            _transactionLogs = database.GetCollection<AuditLogDocument>("TransactionLogs");
            _monthlyProcessesLogs = database.GetCollection<AuditLogDocument>("MonthlyProcessesLogs");
            _errorLogs = database.GetCollection<AuditLogDocument>("ErrorLogs");
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

            if (action.Contains("Login", StringComparison.OrdinalIgnoreCase))
            {
                await _loginLogs.InsertOneAsync(log);
            }
            else if (action.Contains("Transaction", StringComparison.OrdinalIgnoreCase))
            {
                await _transactionLogs.InsertOneAsync(log);
            }
            else if (action.Contains("MonthlyProcess", StringComparison.OrdinalIgnoreCase))
            {
                await _monthlyProcessesLogs.InsertOneAsync(log);
            }
            else
            {
                await _transactionLogs.InsertOneAsync(log);
            }
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
            await _errorLogs.InsertOneAsync(log);
        }
    }
}
