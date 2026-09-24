using OOP_NoSQL.Domain.Entities;
using OOP_NoSQL.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OOP_NoSQLProject
{
    public class ConsoleHandler
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountRepository _accountRepository;
        private readonly IAuditLogger _auditLogger;

        public ConsoleHandler(ITransactionService transactionService, IAccountRepository accountRepository, IAuditLogger auditLogger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task DepositAsync()
        {
            Console.Write("\nInput account number for deposit: ");
            string? accountNumber = Console.ReadLine();
            Console.Write("Input amount to deposit: ");
            if (decimal.TryParse(Console.ReadLine()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                var tx = await _transactionService.DepositAsync(accountNumber ?? "", amount);
                PrintTransactionResult(tx);
            }
            else
            {
                Console.WriteLine("Input is wrong");
                Console.ReadLine();
            }
        }

        public async Task WithdrawAsync()
        {
            Console.Write("\nInput account number for withdrawal: ");
            string? accountNumber = Console.ReadLine();
            Console.Write("Input amount to withdraw: ");
            if (decimal.TryParse(Console.ReadLine()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                var tx = await _transactionService.WithdrawAsync(accountNumber ?? "", amount);
                PrintTransactionResult(tx);
            }
            else
            {
                Console.WriteLine("Input is wrong");
                Console.ReadLine();
            }
        }

        public async Task TransferAsync()
        {
            Console.Write("\nInput source account number for transfer: ");
            string? sourceAccountNumber = Console.ReadLine();
            Console.Write("Input destination account number for transfer: ");
            string? destinationAccountNumber = Console.ReadLine();
            Console.Write("Input amount to transfer: ");
            if (decimal.TryParse(Console.ReadLine()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                var tx = await _transactionService.TransferAsync(sourceAccountNumber ?? "", destinationAccountNumber ?? "", amount);
                PrintTransactionResult(tx);
            }
            else
            {
                Console.WriteLine("Input is wrong");
                Console.ReadLine();
            }
        }

        public async Task MonthlyProcessAsync(BankAccount account1, BankAccount account2)
        {
            account1.ApplyMonthlyProcess();
            account2.ApplyMonthlyProcess();

            await _accountRepository.UpdateAccountAsync(account1);
            await _accountRepository.UpdateAccountAsync(account2);

            await _auditLogger.LogActivityAsync(account1.OwnerId.ToString(), "Monthly process applied.");

            Console.WriteLine("\nMonthly process applied succesfully.");
            Console.WriteLine($"From Checking Account {account1.AccountNumber} New Balance: {account1.Balance} €");
            Console.WriteLine($"From Savings Account {account2.AccountNumber} New Balance: {account2.Balance} €");
            Console.ReadLine();
        }

        public async Task HistoryAsync()
        {
            Console.Write("\nInput account number to view transaction history: ");
            string? accountNumber = Console.ReadLine();
            var transactions = await _transactionService.GetTransactionsHistoryAsync(accountNumber ?? "");
            if (transactions == null || !transactions.Any())
            {
                Console.WriteLine("No transactions found for this account.");
                return;
            }
            Console.WriteLine($"\nTransaction History for Account {accountNumber}:");
            foreach (var tx in transactions)
            {
                Console.WriteLine($"- {tx.Timestamp}: {tx.Type} of {tx.Amount} € - Status: {tx.Status}");
            }
            Console.ReadLine();
        }

        private void PrintTransactionResult(Transaction tx)
        {
            if (tx.Status == TransactionStatus.Completed)
            {
                Console.WriteLine($"\nTransaction successful: {tx.Type} of {tx.Amount} € is made.");
            }
            else
            {
                Console.WriteLine($"\nTransaction failed: {tx.ErrorMessage}");
            }
            Console.ReadLine();
        }

    }
}
