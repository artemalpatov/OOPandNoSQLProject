using System;
using System.Collections.Generic;
using System.Text;

namespace OOP_NoSQL.Domain.Interfaces
{
    public interface IAuditLogger
    {
        Task LogActivityAsync(string userId, string action, object? details = null);

        Task LogErrorAsync(string action, string errorMessage);
    }
}
