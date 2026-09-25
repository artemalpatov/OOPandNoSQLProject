using OOP_NoSQL.Domain.Entities;
using OOP_NoSQL.Domain.Interfaces;
using OOP_NoSQL.Infrastructure;
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

        public async Task<User?> UserAccessAsync()
        {
            Console.WriteLine("Welcome to the Bank System");
            Console.WriteLine("Create New User or Login To Existing One");
            Console.WriteLine("1. Create New User");
            Console.WriteLine("2. Login To Existing User");
            Console.WriteLine("0. Exit");
            Console.WriteLine("================================");

            var choice = Console.ReadLine();

            switch(choice)
            {
                case "1":
                    Console.WriteLine("Please enter your full name: ");
                    var fullName = Console.ReadLine();
                    Console.Write("Please enter your email: ");
                    var newEmail = Console.ReadLine();
                    Console.Write("Please enter your password: ");
                    var newPassword = Console.ReadLine();

                    var newUser = new User(fullName ?? "", newEmail ?? "", newPassword ?? "");
                    newUser.IsLoggedIn = true;
                    await _auditLogger.LogActivityAsync(newUser.Id.ToString(), "User created", new { newUser.FullName, newUser.Email });
                    return newUser;
                case "2":
                    Console.Write("Please enter your email: ");
                    var email = Console.ReadLine();
                    Console.Write("Please enter your password: ");
                    var password = Console.ReadLine();

                    var user = await _accountRepository.GetUserByEmailAsync(email ?? "");
                    if (user != null)
                    {
                        user.IsLoggedIn = true;
                        await _auditLogger.LogActivityAsync(user.Id.ToString(), "User logged in", new { user.FullName, user.Email });

                        return user;
                    }
                    else
                    {
                        Console.WriteLine("User not found. Please try again.");
                        return null;
                    }
                case "0":
                    Console.WriteLine("Exiting the application. Goodbye!");
                    Environment.Exit(0);
                    return null;

                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    return null;
            }
        }

        public async Task<(CheckingAccount?, SavingsAccount?)> CreateAccountsAsync(User user)
        {
            Console.WriteLine("Creating Checking and Savings Accounts for the user...");
            Console.WriteLine("1. Create Checking Account");
            Console.WriteLine("2. Create Savings Account");
            Console.WriteLine("")
            Console.WriteLine("================================");

            var choice = Console.ReadLine();

            string accountSuffix = user.Id.ToString().Replace("-", "").Substring(0, 4).ToUpper();

            switch (choice)
            {
                case "1":
                    var checkingAccount = new CheckingAccount($"C-{accountSuffix}", user.Id, initialBalance: 100m, overdraftLimit: 200m);
                    await _accountRepository.AddAccountAsync(checkingAccount);
                    await _auditLogger.LogActivityAsync(user.Id.ToString(), "Checking account created", new { checkingAccount.AccountNumber });
                    Console.WriteLine($"Checking account created with account number: {checkingAccount.AccountNumber}");
                    return (checkingAccount, null);
                case "2":
                    var savingsAccount = new SavingsAccount($"S-{accountSuffix}", user.Id, initialBalance: 100m, interestRate: 0.05m);
                    await _accountRepository.AddAccountAsync(savingsAccount);
                    await _auditLogger.LogActivityAsync(user.Id.ToString(), "Savings account created", new { savingsAccount.AccountNumber });
                    Console.WriteLine($"Savings account created with account number: {savingsAccount.AccountNumber}");
                    return (null, savingsAccount);
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    return (null, null);
            }
        }
    }
}
