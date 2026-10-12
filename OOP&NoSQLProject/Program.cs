using OOP_NoSQL.Domain.Entities;
using OOP_NoSQL.Domain.Interfaces;
using OOP_NoSQL.Infrastructure;
using OOP_NoSQLProject;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace OOP_NoSQL.ConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Banking System";

            using var DbContext = new BankDbContext();
            IAccountRepository accountRepository = new SqlAccountRepository(DbContext);
            IAuditLogger auditLogger = new MongoAuditLogger("mongodb://localhost:27017", "BankingAuditDb");
            ITransactionService transactionService = new TransactionService(accountRepository, auditLogger, DbContext);

            var Handler = new ConsoleHandler(transactionService, accountRepository, auditLogger);

            User? user = null;

            string? savedEmail = await SessionManager.GetSavedSessionEmailAsync();
            if (!string.IsNullOrWhiteSpace(savedEmail))
            {
                user = await accountRepository.GetUserByEmailAsync(savedEmail);
                if (user != null)
                {
                    Console.WriteLine($"Welcome Back, {user.FullName}");
                }
            }

            while (user == null)
            {
                Console.Clear();
                user = await Handler.UserAccessAsync();
            }

            var existingAccount = (await accountRepository.GetAccountByUserIdAsync(user.Id)).ToList();

            CheckingAccount? checking = existingAccount.OfType<CheckingAccount>().FirstOrDefault();
            SavingsAccount? savings = existingAccount.OfType<SavingsAccount>().FirstOrDefault();

            if (!existingAccount.Any())
            {
                Console.WriteLine("\nNo Active Accounts. Let's Create The First Account");
                Console.ReadLine();
                await Handler.CreateAccountsAsync(user);
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine($"  Bank System | Client: {user.FullName}");
                Console.WriteLine("==================================================");
                Console.WriteLine(checking != null ? $" Checking Account [{checking.AccountNumber}]: {checking.Balance:F2} € (Overdraft Limit: {checking.OverdraftLimit:F2} €)" : " No Checking Account");
                Console.WriteLine(savings != null ? $" Savings Account [{savings.AccountNumber}]: {savings.Balance:F2} € (Rate: {savings.InterestRate * 100}%)" : " No Savings Account");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine(" 1. Deposit Funds");
                Console.WriteLine(" 2. Withdraw Cash");
                Console.WriteLine(" 3. Transfer Funds Between Accounts");
                Console.WriteLine(" 4. Apply Monthly Process");
                Console.WriteLine(" 5. View Transaction History");
                Console.WriteLine(" 6. Create One More Account");
                Console.WriteLine(" 9. Log out");
                Console.WriteLine(" 0. Exit");
                Console.WriteLine("==================================================");
                Console.Write("Choose an action: ");

                string? choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        await Handler.DepositAsync();
                        break;
                    case "2":
                        await Handler.WithdrawAsync();
                        break;
                    case "3":
                        await Handler.TransferAsync();
                        break;
                    case "4":
                        if (checking != null) await Handler.MonthlyProcessAsync(checking);
                        if (savings != null) await Handler.MonthlyProcessAsync(savings);
                        break;
                    case "5":   
                        await Handler.HistoryAsync();
                        break;
                    case "6":
                        (checking, savings) = await Handler.CreateAccountsAsync(user);
                        break;
                    case "9":
                        SessionManager.ClearSession();
                        Console.WriteLine("Logging out...");
                        Console.Clear();
                        user = await Handler.UserAccessAsync();
                        while(user == null)
                        {
                            Console.Clear();
                            user = await Handler.UserAccessAsync();
                        }
                        (checking, savings) = await Handler.CreateAccountsAsync(user);
                       
                        break;
                    case "0":
                        Console.WriteLine("Exiting the application...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}