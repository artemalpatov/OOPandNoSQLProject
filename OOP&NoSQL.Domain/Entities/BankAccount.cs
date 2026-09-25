using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQL.Domain.Entities
{
    public abstract class BankAccount
    {
        public Guid Id { get; protected set; }
        public string AccountNumber { get; protected set; }
        public Guid OwnerId { get; protected set; }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            protected set  => _balance = value;
        }

        protected BankAccount() { } // For EF Core

        protected BankAccount(string accountNumber, Guid ownerId, decimal initialBalance)
        {
            if (initialBalance < 0)
            {
                throw new ArgumentException("Initial balance cannot be negative.");
            }

            Id = Guid.NewGuid();
            AccountNumber = accountNumber;
            OwnerId = ownerId;
            _balance = initialBalance;
        }

        public virtual void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be positive.");
            }
            _balance += amount;
        }

        public abstract bool Withdraw(decimal amount);

        public virtual void ApplyMonthlyProcess()
        {

        }
    }
}
