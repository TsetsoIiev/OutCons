using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutCons.Web.Models;

namespace OutCons.Web.Interfaces
{
    public interface IOutConsService
    {
        Task<List<User>> GetUsersAsync(string searchText, string sortOrder, int? pageNumber, CancellationToken cancellationToken);

        Task<int> GetAllUsersCountAsync(CancellationToken cancellationToken);

        Task<bool> GenerateNewDbData(CancellationToken cancellationToken);
        
        List<TimeLogReport> GetTimeLogsForPerson(int userId, CancellationToken cancellationToken);
        
        List<TimeLogReport> GetMostActiveUsers(string filter);
    }
}