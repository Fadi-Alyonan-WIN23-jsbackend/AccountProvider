using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions
{
    public class GetAllAdmins
    {
        private readonly ILogger<GetAllAdmins> _logger;
        private readonly UserManager<UserAccount> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _context;
        public GetAllAdmins(ILogger<GetAllAdmins> logger, UserManager<UserAccount> userManager, RoleManager<IdentityRole> roleManager, DataContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [Function("GetAllAdmins")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            try
            {

                var adminRole = await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == "admin");

                if (adminRole == null)
                {
                    return new NotFoundObjectResult("Admin role not found");
                }


                var adminUserIds = await _context.UserRoles
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                if (adminUserIds.Any())
                {
                    var adminUsers = await _userManager.Users
                        .Where(u => adminUserIds.Contains(u.Id)).Select(u => new
                        {
                            u.Id,
                            u.FirstName,
                            u.LastName,
                            u.Email,
                            u.PhoneNumber,
                            Role = "admin"
                        })
                        .ToListAsync();

                    var json = JsonConvert.SerializeObject(adminUsers);
                    return new OkObjectResult(json);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get All Admins :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
