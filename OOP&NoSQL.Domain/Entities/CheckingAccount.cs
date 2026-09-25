using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQL.Domain.Entities
{
    public class CheckingAccount : BankAccount
    {
        public decimal OverdraftLimit { get; private set; }
        public decimal MonthlyFee { get; private set; }

        protected CheckingAccount() : base() { } // For EF Core

        public CheckingAccount(
            string accountNumber,
            Guid ownerId,
            decimal initialBalance,
            decimal overdraftLimit = 500m,
            decimal monthlyFee = 10m
            ) : base(accountNumber, ownerId, initialBalance)
        {
            if (overdraftLimit < 0) 
                throw new ArgumentException("Overdraft limit cannot be negative.");

            OverdraftLimit = overdraftLimit;
            MonthlyFee = monthlyFee;
        }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive.");
            
            if (Balance + OverdraftLimit >= amount)
            {
                Balance -= amount;
                return true;
            }

            return false;
        }

        public override void ApplyMonthlyProcess()
        {
            Balance -= MonthlyFee;
        }
    }
}
