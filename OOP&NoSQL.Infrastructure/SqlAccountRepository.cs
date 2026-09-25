using System;
using System.Collections.Generic;
using System.Text;
using OOP_NoSQL.Domain.Entities;
using OOP_NoSQL.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace OOP_NoSQL.Infrastructure
{
    public class SqlAccountRepository : IAccountRepository
    {
        private readonly BankDbContext _db;

        public SqlAccountRepository(BankDbContext db)
        {
            _db = db;
            _db.Database.EnsureCreated();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAccountAsync(BankAccount account)
        {
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();
        }

        public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<BankAccount>> GetAccountByUserIdAsync(Guid userId)
        {
            return await _db.Accounts.Where(a => a.OwnerId == userId).ToListAsync();
        }

        public async Task UpdateAccountAsync(BankAccount account)
        {
            _db.Accounts.Update(account);
            await _db.SaveChangesAsync();
        }


    }
}
