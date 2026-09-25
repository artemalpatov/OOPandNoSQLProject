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
            ITransactionService transactionService = new TransactionService(accountRepository, auditLogger);

            var Handler = new ConsoleHandler(transactionService, accountRepository, auditLogger);

           
            var user = await Handler.UserAccessAsync();
            if (user == null)
            {
                Console.WriteLine("Failed to access user account.");
                return;
            }

            var (checking, savings) = await Handler.CreateAccountsAsync(user);
            if (checking == null || savings == null)
            {
                Console.WriteLine("Failed to create accounts.");
                return;
            }

            

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine($"  Bank System | Client: {user.FullName}");
                Console.WriteLine("==================================================");
                Console.WriteLine($" 1. Checking Account [{checking.AccountNumber}]: {checking.Balance:F2} € (Overdraft Limit: {checking.OverdraftLimit:F2} €)");
                Console.WriteLine($" 2. Savings Account [{savings.AccountNumber}]: {savings.Balance:F2} € (Rate: {savings.InterestRate * 100}%)");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine(" 1. Deposit Funds");
                Console.WriteLine(" 2. Withdraw Cash");
                Console.WriteLine(" 3. Transfer Funds Between Accounts");
                Console.WriteLine(" 4. Apply Monthly Process");
                Console.WriteLine(" 5. View Transaction History");
                Console.WriteLine(" 9. Log out");
                Console.WriteLine(" 0. Exit");
                Console.WriteLine("==================================================");
                Console.Write("Choose an action: ");

                string? choice = Console.ReadLine();

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
                        await Handler.MonthlyProcessAsync(checking, savings);
                        break;
                    case "5":   
                        await Handler.HistoryAsync();
                        break;
                    case "9":
                        Console.WriteLine("Logging out...");
                        user = await Handler.UserAccessAsync();
                        if (user == null)
                        {
                            Console.WriteLine("Failed to access user account.");
                            return;
                        }
                        (checking, savings) = await Handler.CreateAccountsAsync(user);
                        if (checking == null || savings == null)
                        {
                            Console.WriteLine("Failed to create accounts.");
                            return;
                        }
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