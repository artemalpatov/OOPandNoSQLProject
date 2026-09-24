using System;
using System.Collections.Generic;
using System.Text;
using OOP_NoSQL.Domain.Entities;
using OOP_NoSQL.Domain.Interfaces;

namespace OOP_NoSQL.Infrastructure
{
    public class InMemoryAccountRepository : IAccountRepository
    {
        private readonly List<BankAccount> _accounts = new();

        public Task<BankAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            return Task.FromResult(account);
        }

        public Task<IEnumerable<BankAccount>> GetAccountByUserIdAsync(Guid userId)
        {
            var userAccounts = _accounts.Where(a => a.OwnerId == userId);
            return Task.FromResult(userAccounts);
        }

        public Task AddAccountAsync(BankAccount account)
        {
            _accounts.Add(account);
            return Task.CompletedTask;
        }

        public Task UpdateAccountAsync(BankAccount account)
        {
            
            return Task.CompletedTask;
        }

    }
}
