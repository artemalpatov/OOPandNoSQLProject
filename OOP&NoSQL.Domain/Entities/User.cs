using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQL.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }

        private readonly List<BankAccount> _accounts = new();
        public IReadOnlyCollection<BankAccount> Accounts => _accounts.AsReadOnly();

        public User(string fullName, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Full name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                throw new ArgumentException("Email cannot be empty.");
            }

            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
        }

        public void AddAccount(BankAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (account.OwnerId != Id)
            {
                throw new InvalidOperationException("Account owner does not match user.");
            }

            _accounts.Add(account);
        }

        public bool RemoveAccount(Guid accountId)
        {
            var account = _accounts.Find(a => a.Id == accountId);
            if (account == null)
                return false;

            return _accounts.Remove(account);
        }
    }
}
