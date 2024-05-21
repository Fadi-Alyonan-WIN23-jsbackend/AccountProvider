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
    public class GetAllUsers
    {
        private readonly ILogger<GetAllUsers> _logger;
        private readonly UserManager<UserAccount> _userManager;
        public GetAllUsers(ILogger<GetAllUsers> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("GetAllUsers")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            try
            {
                var users = _userManager.Users;

                if (users != null)
                {
                    var userList = await users.ToListAsync();
                    var json = JsonConvert.SerializeObject(userList);
                    return new OkObjectResult(json);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" Get All Users :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
