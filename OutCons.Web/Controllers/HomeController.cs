using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutCons.Web.Interfaces;
using OutCons.Web.Models;

namespace OutCons.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOutConsService _outConsService;

        public HomeController(IOutConsService outConsService)
        {
            _outConsService = outConsService;
        }

        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchText, 
            int? pageNumber,
            CancellationToken cancellationToken)
        {
            ViewData["FirstNameSortParam"] = string.IsNullOrWhiteSpace(sortOrder) ? "FirstNameDescending" : string.Empty;
            ViewData["LastNameSortParam"] = sortOrder == "LastNameDescending" ? "LastNameDescending" : "LastName";
            ViewData["EmailSortParam"] = sortOrder == "Email" ? "EmailDescending" : "Email";
            ViewData["CurrentFilter"] = searchText;
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                pageNumber = Constants.FirstPage;
            }
            else
            {
                searchText = currentFilter;
            }

            var usersFiltered = await _outConsService.GetUsersAsync(searchText, sortOrder, pageNumber, cancellationToken);
            var allUsersCount = await _outConsService.GetAllUsersCountAsync(cancellationToken);
            var totalPages = (int) Math.Ceiling(allUsersCount / (double) Constants.PageSize);

            
            return View(new IndexModel()
            {
                Users = usersFiltered,
                PageIndex = pageNumber ?? Constants.FirstPage - 1,
                TotalPages = totalPages
            });
        }

        [HttpGet]
        public IActionResult ViewProjectHours(int? userId, CancellationToken cancellationToken)
        {
            if (userId is null)
            {
                return BadRequest();
            }
            
            var timeLogsForPerson = _outConsService.GetTimeLogsForPerson(userId.Value, cancellationToken);

            return timeLogsForPerson is not null
                ? Ok(timeLogsForPerson)
                : NoContent();
        }

        [HttpGet]
        public IActionResult GetMostActiveUsers(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                filter = "project";
            }

            var users = _outConsService.GetMostActiveUsers(filter);
            
            return users is not null
                ? Ok(users)
                : NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> RefreshDatabase(CancellationToken cancellationToken)
        {
            var result = await _outConsService.GenerateNewDbData(cancellationToken);

            return Ok(result);
        }
    }
}
