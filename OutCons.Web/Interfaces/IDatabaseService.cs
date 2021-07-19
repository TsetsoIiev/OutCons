using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutCons.Web.Models;

namespace OutCons.Web.Interfaces
{
    public interface IDatabaseService
    {
        bool TruncateDatabase();
        
        
        Task<bool> GenerateNewDataAsync(CancellationToken cancellationToken);

        Task<List<User>> GetUsersPaginatedAsync(string searchText, int? pageNumber, CancellationToken cancellationToken);

        Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken);

        List<TimeLogReport> GetTimeLogsForPerson(int userId, CancellationToken cancellationToken);
        
        List<TimeLogReport> GetMostActiveUsers(string filter);
    }
}
