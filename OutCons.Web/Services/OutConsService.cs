using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutCons.Web.Enums;
using OutCons.Web.Interfaces;
using OutCons.Web.Models;
using static System.Enum;

namespace OutCons.Web.Services
{
    public class OutConsService : IOutConsService
    {
        private readonly IDatabaseService _db;
        
        public OutConsService(IDatabaseService db)
        {
            _db = db;
        }
        
        public async Task<List<User>> GetUsersAsync(string searchText, string sortOrderStr, int? pageNumber, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
            }
            
            var users = await _db.GetUsersPaginatedAsync(searchText, pageNumber, cancellationToken);

            TryParse(sortOrderStr, out SortOrder sortOrder);
            
            users = sortOrder switch
            {
                SortOrder.FirstNameDescending => users.OrderByDescending(u => u.FirstName).ToList(),
                SortOrder.LastName => users.OrderBy(u => u.LastName).ToList(),
                SortOrder.LastNameDescending => users.OrderByDescending(u => u.LastName).ToList(),
                SortOrder.Email => users.OrderBy(u => u.Email).ToList(),
                SortOrder.EmailDescending => users.OrderByDescending(u => u.Email).ToList(),
                _ => users.OrderBy(u => u.FirstName).ToList()
            };

            return users;
        }

        public async Task<int> GetAllUsersCountAsync(CancellationToken cancellationToken)
        {
            var users = await _db.GetAllUsersAsync(cancellationToken);

            return users?.Count ?? 0;
        }

        public async Task<bool> GenerateNewDbData(CancellationToken cancellationToken)
        {
            return _db.TruncateDatabase()
                && await _db.GenerateNewDataAsync(cancellationToken);
        }

        public List<TimeLogReport> GetTimeLogsForPerson(int userId, CancellationToken cancellationToken)
        {
            return _db.GetTimeLogsForPerson(userId, cancellationToken);
        }

        public List<TimeLogReport> GetMostActiveUsers(string filter)
        {
            return _db.GetMostActiveUsers(filter);
        }
    }
}