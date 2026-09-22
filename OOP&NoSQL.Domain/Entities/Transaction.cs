using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQL.Domain.Entities
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed
    }

    public class Transaction
    {
        public Guid Id { get; private set; }
        public string? FromAccountNumber { get; private set; }
        public string? ToAccountNumber { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime Timestamp { get; private set; }
        public TransactionType Type { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string ErrorMessage { get; private set; }

        public Transaction(string? fromAccountNumber, string? toAccountNumber, decimal amount, TransactionType type)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Transaction amount must be positive.");
            }

            Id = Guid.NewGuid();
            FromAccountNumber = fromAccountNumber;
            ToAccountNumber = toAccountNumber;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
            Type = type;
            Status = TransactionStatus.Pending;
        }

        public void MarkAsCompleted()
        {
            Status = TransactionStatus.Completed;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = TransactionStatus.Failed;
            ErrorMessage = errorMessage;
        }
    }
}
