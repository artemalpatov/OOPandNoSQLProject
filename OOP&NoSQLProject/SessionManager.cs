using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQLProject
{
    public static class SessionManager
    {
        private static readonly string SessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");

        public static async Task SaveSessionAsync(string email)
        {
            await File.WriteAllTextAsync(SessionFilePath, email);
        }

        public static async Task<string?> GetSavedSessionEmailAsync()
        {
            if (File.Exists(SessionFilePath))
            {
                string email = await File.ReadAllTextAsync(SessionFilePath);
                return string.IsNullOrWhiteSpace(email) ? null : email.Trim();
            }
            return null;
        }

        public static void ClearSession()
        {
            if (File.Exists(SessionFilePath))
            {
                File.Delete(SessionFilePath);
            }
        }
    }
}
