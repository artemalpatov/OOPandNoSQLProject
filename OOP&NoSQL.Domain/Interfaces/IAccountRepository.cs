using OOP_NoSQL.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQL.Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task AddUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<BankAccount?> GetByAccountNumberAsync(string accountNumber);
        Task<IEnumerable<BankAccount>> GetAccountByUserIdAsync(Guid userId);
        Task AddAccountAsync(BankAccount account);
        Task UpdateAccountAsync(BankAccount account);

    }
}
