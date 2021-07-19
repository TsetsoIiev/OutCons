using Bogus;
using OutCons.Web.Extensions;
using OutCons.Web.Interfaces;
using OutCons.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OutCons.Web.Services
{
    internal class DatabaseService : IDatabaseService
    {
        private readonly OutConsContext _context;
        private readonly Randomizer _randomizer;

        private const int PeopleToGenerateCount = 100;
        private const int MinGeneratedTimeLogsPerPerson = 1;
        private const int MaxGeneratedTimeLogsPerPerson = 20;
        private const double MaxHoursPerWorkingProject = 8f;

        public DatabaseService(OutConsContext context)
        {
            _context = context;
            _randomizer = new Randomizer();
        }

        public bool TruncateDatabase()
        {
            try
            {
                //// Truncate also works :)
                _context.Users.RemoveEntities();
                _context.Projects.RemoveEntities();
                _context.TimeLogs.RemoveEntities();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> GenerateNewDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                var domains = new[] {"hotmail.com", "gmail.com", "live.com"};

                var users = new Faker<User>()
                    .RuleFor(u => u.FirstName, f => f.Person.FirstName)
                    .RuleFor(u => u.LastName, f => f.Person.LastName)
                    .RuleFor(u => u.Email, (f, u) => $"{u.FirstName}.{u.LastName}@{f.PickRandom(domains)}")
                    .Generate(PeopleToGenerateCount);
                await _context.Users.AddRangeAsync(users, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var projects = new List<Project>()
                {
                    new() {Name = "My own"},
                    new() {Name = "OutCons"},
                    new() {Name = "Free time"}
                };
                await _context.Projects.AddRangeAsync(projects, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var timeLogs = new List<TimeLog>();
                var outConsProject = await _context.Projects.FirstOrDefaultAsync(p => p.Name == "OutCons", cancellationToken);
                
                foreach (var user in users)
                {
                    var timeLogsForUser = GenerateTimeLogsForPerson(
                        user.Id,
                        projects.Select(x => x.Id).ToArray());

                    timeLogsForUser = CheckWorkingTimeForUser(timeLogsForUser.ToList(), outConsProject);
                    
                    timeLogs.AddRange(timeLogsForUser);
                }

                await _context.TimeLogs.AddRangeAsync(timeLogs, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }         
        }

        public async Task<List<User>> GetUsersPaginatedAsync(string searchText, int? pageNumber, CancellationToken cancellationToken)
        {
            var users = await _context.Users
                .AsQueryable()
                .Skip((pageNumber ?? Constants.FirstPage - 1) * Constants.PageSize)
                .Take(Constants.PageSize)
                .ToListAsync(cancellationToken);
            
            return users;
        }

        public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            return await _context.Users.ToListAsync(cancellationToken);
        }

        public List<TimeLogReport> GetTimeLogsForPerson(int userId, CancellationToken cancellationToken)
        {
            var timeLogs = _context.TimeLogs
                .Where(tl => tl.UserId == userId)
                .AsEnumerable()
                .GroupBy(tl => tl.ProjectId)
                .ToDictionary(group => group.Key, group => group.ToList())
                .Select(tl => new TimeLogReport()
                {
                    FilteredById = tl.Value.FirstOrDefault().Id,
                    TotalHours = tl.Value.Sum(x => x.Hours),
                })
                .ToList();

            return timeLogs;
        }

        public List<TimeLogReport> GetMostActiveUsers(string filter)
        {
            return _context.TimeLogs
                .AsEnumerable()
                .GroupBy(t => filter == "project" ? t.ProjectId : t.UserId)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Hours))
                .OrderByDescending(g => g.Value)
                .Take(10)
                .Select(tl => new TimeLogReport()
                {
                    FilteredById = tl.Key,
                    TotalHours = tl.Value
                }).ToList();
        }

        private IEnumerable<TimeLog> GenerateTimeLogsForPerson(int userId, int[] projectIds)
        {
            var timeLogs = new Faker<TimeLog>()
                    .RuleFor(t => t.Date, f => f.Date.Between(DateTime.Now.AddDays(-2), DateTime.Now).Date)
                    .RuleFor(t => t.Hours, f => (double)decimal.Round(f.Random.Decimal(0.25m, 8.0m), 2))
                    .RuleFor(t => t.UserId, f => userId)
                    .RuleFor(t => t.ProjectId, f => projectIds[_randomizer.Int(0, projectIds.Length - 1)])
                    .Generate(_randomizer.Int(MinGeneratedTimeLogsPerPerson, MaxGeneratedTimeLogsPerPerson));

            return timeLogs;
        }

        private IEnumerable<TimeLog> CheckWorkingTimeForUser(List<TimeLog> timeLogsForUser, Project outConsProject)
        {
            var timeLogs = timeLogsForUser
                .GroupBy(t => t.Date)
                .ToList();

            foreach (var timeLogGroup in timeLogs)
            {
                double timeLogged = 0f;
                var timeLogsPerDay = timeLogGroup.ToList();

                foreach (var tl in timeLogsPerDay)
                {
                    if (tl.ProjectId != outConsProject.Id) continue;

                    if (tl.Hours + timeLogged <= MaxHoursPerWorkingProject) continue;
                    
                    timeLogged = MaxHoursPerWorkingProject;
                    tl.Hours = MaxHoursPerWorkingProject - timeLogged;
                }
            }

            return timeLogsForUser;
        }
    }
}
