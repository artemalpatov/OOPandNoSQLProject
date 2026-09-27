using OOP_NoSQL.Domain.Entities;
using OOP_NoSQL.Domain.Interfaces;
using OOP_NoSQL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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

        public async Task MonthlyProcessAsync(BankAccount account1)
        {
            account1.ApplyMonthlyProcess();

            await _accountRepository.UpdateAccountAsync(account1);

            await _auditLogger.LogActivityAsync(account1.OwnerId.ToString(), $"Monthly process applied for account {account1.AccountNumber}.");

            Console.WriteLine("\nMonthly process applied succesfully.");
            Console.WriteLine($"From Checking Account {account1.AccountNumber} New Balance: {account1.Balance} €");
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
                    if (fullName == null || fullName.Length <= 3 || fullName.Length >= 15)
                    {
                        Console.WriteLine("Name must be between 3 and 15 symbols. Please try again.");
                        Console.ReadLine();
                        return null;
                    }
                    Console.Write("Please enter your email: ");
                    var newEmail = Console.ReadLine()?.Trim() ?? "";
                    if (!Regex.IsMatch(newEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("You need to input valid email format. Please try again.");
                        Console.ReadLine();
                        return null;
                    }
                    var existingUser = await _accountRepository.GetUserByEmailAsync(newEmail ?? "");
                    if (existingUser != null)
                    {
                        Console.WriteLine("User with this email already exists. Please try again.");
                        Console.ReadLine();
                        return null;
                    }
                    Console.Write("Please enter your password: ");
                    var newPassword = Console.ReadLine()?.Trim();
                    if (newPassword == null || newPassword == "")
                    {
                        Console.WriteLine("You need to set password. Please try again.");
                        Console.ReadLine();
                        return null;
                    }

                    var newUser = new User(fullName, newEmail ?? "", newPassword);
                    await _accountRepository.AddUserAsync(newUser);
                    await _auditLogger.LogActivityAsync(newUser.Id.ToString(), "User created", new { newUser.FullName, newUser.Email });
                    await SessionManager.SaveSessionAsync(newUser.Email);
                    return newUser;
                case "2":
                    Console.Write("Please enter your email: ");
                    var email = Console.ReadLine();
                    var emailExists = await _accountRepository.GetUserByEmailAsync(email ?? "");
                    if (emailExists == null)
                    {
                        Console.WriteLine("There are no users with this email. Please try again.");
                        Console.ReadLine();
                        return null;
                    }
                    Console.Write("Please enter your password: ");
                    var password = Console.ReadLine();

                    var user = await _accountRepository.GetUserByEmailAsync(email ?? "");
                    if (user != null && password == user.PasswordHash)
                    {
                        await _auditLogger.LogActivityAsync(user.Id.ToString(), "User logged in", new { user.FullName, user.Email });
                        await SessionManager.SaveSessionAsync(user.Email);
                        return user;
                    }
                    else if (user != null)
                    {
                        Console.WriteLine("Password is wrong. Please try again.");
                        Console.ReadLine();
                        return null;
                    }
                    else
                    {
                        Console.WriteLine("User not found. Please try again.");
                        Console.ReadLine();
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
            CheckingAccount? checkingAccount = null;
            SavingsAccount? savingsAccount = null;

            string accountSuffix = user.Id.ToString().Replace("-", "").Substring(0, 4).ToUpper();

            while (true)
            {
                var existingAccounts = await _accountRepository.GetAccountByUserIdAsync(user.Id);

                bool hasChecking = existingAccounts.OfType<CheckingAccount>().Any() || checkingAccount != null;
                bool hasSavings = existingAccounts.OfType<SavingsAccount>().Any() || savingsAccount != null;

                Console.Clear();
                Console.WriteLine($"=== Manage Accounts for {user.FullName} ===");
                Console.WriteLine(hasChecking ? "1. [CREATED] Checking Account" : "1. Create Checking Account");
                Console.WriteLine(hasSavings ? "2. [CREATED] Savings Account" : "2. Create Savings Account");
                Console.WriteLine("3. Done & Continue to Main Menu");
                Console.WriteLine("0. Exit Application");
                Console.WriteLine("=======================================");
                Console.Write("Choose an action: ");

                var choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        if (hasChecking)
                        {
                            Console.WriteLine("You already have a Checking Account. Please choose another option.");
                            Console.ReadLine();
                            break;
                        }   


                        var createdChecking = new CheckingAccount($"C-{accountSuffix}", user.Id, initialBalance: 100m, overdraftLimit: 200m);
                        await _accountRepository.AddAccountAsync(createdChecking);
                        await _auditLogger.LogActivityAsync(user.Id.ToString(), "Checking account created", new { createdChecking.AccountNumber });

                        Console.WriteLine($"Checking account created with account number: {createdChecking.AccountNumber}");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadLine();
                        break;
                    case "2":
                        if (hasSavings)
                        {
                            Console.WriteLine("You already have a Savings Account. Please choose another option.");
                            Console.ReadLine();
                            break;
                        }

                        var createdSavings = new SavingsAccount($"S-{accountSuffix}", user.Id, initialBalance: 100m, interestRate: 0.05m);
                        await _accountRepository.AddAccountAsync(createdSavings);
                        await _auditLogger.LogActivityAsync(user.Id.ToString(), "Savings account created", new { createdSavings.AccountNumber });

                        Console.WriteLine($"Savings account created with account number: {createdSavings.AccountNumber}");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadLine();
                        break;

                    case "3":
                        var allUserAccounts = (await _accountRepository.GetAccountByUserIdAsync(user.Id)).ToList();

                        if (!allUserAccounts.Any())
                        {
                            Console.WriteLine("You must create at least one account before proceeding.");
                            Console.ReadLine();
                            break;
                        }

                        var finalCheckingAccount = allUserAccounts.OfType<CheckingAccount>().FirstOrDefault() ?? checkingAccount;
                        var finalSavingsAccount = allUserAccounts.OfType<SavingsAccount>().FirstOrDefault() ?? savingsAccount;
                         
                        return (finalCheckingAccount, finalSavingsAccount);


                    case "0":
                        Console.WriteLine("Exiting the application. Goodbye!");
                        Environment.Exit(0);
                        return (null, null);
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        return (null, null);
                }
            }
        }
    }
}
