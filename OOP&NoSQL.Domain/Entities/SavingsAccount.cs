using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQL.Domain.Entities
{
    public class SavingsAccount : BankAccount
    {
        public decimal InterestRate { get; private set; }

        protected SavingsAccount() : base() { } // For EF Core

        public SavingsAccount(
            string accountNumber,
            Guid ownerId,
            decimal initialBalance,
            decimal interestRate = 0.05m
            ) : base(accountNumber, ownerId, initialBalance)
        {
            if (interestRate < 0)
                throw new ArgumentException("Interest rate cannot be negative.");

            InterestRate = interestRate;
        }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive.");

            if(Balance >= amount)
            {
                Balance -= amount;
                return true;
            }

            return false;
        }

        public override void ApplyMonthlyProcess()
        {
            decimal interestAmount = Balance * InterestRate;
            Balance += interestAmount;
        }
    }
}
