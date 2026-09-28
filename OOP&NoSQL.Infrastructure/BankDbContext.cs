using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OOP_NoSQL.Domain.Entities;

namespace OOP_NoSQL.Infrastructure
{
    public class BankDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<BankAccount> Accounts { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=banking.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BankAccount>()
                .HasDiscriminator<string>("AccountType")
                .HasValue<CheckingAccount>("Checking")
                .HasValue<SavingsAccount>("Savings");

            modelBuilder.Entity<BankAccount>()
                .HasKey(a => a.AccountNumber);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Accounts)
                .WithOne()
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Amount).HasConversion<double>(); 
            });
        }
    }
}
