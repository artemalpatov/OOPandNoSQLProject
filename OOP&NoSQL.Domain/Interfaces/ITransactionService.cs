using System;
using System.Collections.Generic;
using System.Text;
using OOP_NoSQL.Domain.Entities;

namespace OOP_NoSQL.Domain.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> TransferAsync(string fromAccountNumber, string toAccountNumber, decimal amount);
        Task<Transaction> DepositAsync(string accountNumber, decimal amount);
        Task<Transaction> WithdrawAsync(string accountNumber, decimal amount);
        Task<IEnumerable<Transaction>> GetTransactionsHistoryAsync(string accountNumber);
    }
}
