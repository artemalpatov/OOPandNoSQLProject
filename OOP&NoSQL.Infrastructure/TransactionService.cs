using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OOP_NoSQL.Domain.Entities;
using OOP_NoSQL.Domain.Interfaces;

namespace OOP_NoSQL.Infrastructure
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAuditLogger _auditLogger;
        private readonly BankDbContext _db;

        public TransactionService(IAccountRepository accountRepository, IAuditLogger auditLogger, BankDbContext db)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Transaction> DepositAsync(string accountNumber, decimal amount)
        {
            var transaction = new Transaction(null, accountNumber, amount, TransactionType.Deposit);

            try
            {
                var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
                if (account == null)
                {
                    string error = $"Account {accountNumber} not found.";
                    transaction.MarkAsFailed(error);
                    await _auditLogger.LogErrorAsync("DEPOSIT", error);
                    return transaction;
                }

                account.Deposit(amount);
                await _accountRepository.UpdateAccountAsync(account);

                transaction.MarkAsCompleted();
                await _db.Transactions.AddAsync(transaction);
                await _db.SaveChangesAsync();

                await _auditLogger.LogActivityAsync(
                    account.OwnerId.ToString(),
                    "DEPOSIT",
                    new { AccountNumber = accountNumber, Amount = amount }
                );

                return transaction;
            }
            catch (Exception ex)
            {
                string detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                transaction.MarkAsFailed(detailedError);
                await _auditLogger.LogErrorAsync("DEPOSIT", detailedError);
                return transaction;
            }
        }

        public async Task<Transaction> WithdrawAsync(string accountNumber, decimal amount)
        {
            var transaction = new Transaction(accountNumber, null, amount, TransactionType.Withdrawal);

            try
            {
                var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);

                if (account == null)
                {
                    string error = $"Account {accountNumber} not found.";
                    transaction.MarkAsFailed(error);
                    await _auditLogger.LogErrorAsync("WITHDRAW", error);
                    return transaction;
                }

                bool success = account.Withdraw(amount);
                if (!success)
                {
                    string error = $"Insufficient funds for withdrawal from account {accountNumber}.";
                    transaction.MarkAsFailed(error);
                    await _auditLogger.LogErrorAsync("WITHDRAW", error);
                    return transaction;
                }

                await _accountRepository.UpdateAccountAsync(account);

                transaction.MarkAsCompleted();
                await _db.Transactions.AddAsync(transaction);
                await _db.SaveChangesAsync();

                await _auditLogger.LogActivityAsync(
                    account.OwnerId.ToString(),
                    "WITHDRAWAL",
                    new { AccountNumber = accountNumber, Amount = amount }
                );

                return transaction;
            }
            catch (Exception ex)
            {
                string detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                transaction.MarkAsFailed(detailedError);
                await _auditLogger.LogErrorAsync("WITHDRAWAL", detailedError);
                return transaction;
            }

        }

        public async Task<Transaction> TransferAsync(string fromAccountNumber, string toAccountNumber, decimal amount)
        {
            var transaction = new Transaction(fromAccountNumber, toAccountNumber, amount, TransactionType.Transfer);

            try
            {
                var fromAccount = await _accountRepository.GetByAccountNumberAsync(fromAccountNumber);
                var toAccount = await _accountRepository.GetByAccountNumberAsync(toAccountNumber);

                if (fromAccount == null)
                {
                    string error = $"The first account not found. From: {fromAccountNumber}.";
                    transaction.MarkAsFailed(error);
                    await _auditLogger.LogErrorAsync("TRANSFER", error);
                    return transaction;
                }
                else if (toAccount == null)
                {
                    string error = $"The second account not found. To: {toAccountNumber}.";
                    transaction.MarkAsFailed(error);
                    await _auditLogger.LogErrorAsync("TRANSFER", error);
                    return transaction;
                }

                bool withdrawSuccess = fromAccount.Withdraw(amount);
                if (!withdrawSuccess)
                {
                    string error = $"Insufficient funds for transfer from account {fromAccountNumber}.";
                    transaction.MarkAsFailed(error);
                    await _auditLogger.LogErrorAsync("TRANSFER", error);
                    return transaction;
                }

                toAccount.Deposit(amount);

                await _accountRepository.UpdateAccountAsync(fromAccount);
                await _accountRepository.UpdateAccountAsync(toAccount);

                transaction.MarkAsCompleted();
                await _db.Transactions.AddAsync(transaction);
                await _db.SaveChangesAsync();

                await _auditLogger.LogActivityAsync(
                    fromAccount.OwnerId.ToString(),
                    "TRANSFER",
                    new { FromAccountNumber = fromAccountNumber, ToAccountNumber = toAccountNumber, Amount = amount }
                );
                return transaction;
            }
            catch (Exception ex)
            {
                string detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                transaction.MarkAsFailed(detailedError);
                await _auditLogger.LogErrorAsync("TRANSFER", detailedError);
                return transaction;
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsHistoryAsync(string accountNumber)
        {
            return await _db.Transactions
                .Where(t => t.FromAccountNumber == accountNumber || t.ToAccountNumber == accountNumber)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
        }
    }
}