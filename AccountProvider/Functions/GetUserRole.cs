using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace AccountProvider.Functions
{
    public class GetUserRole
    {
        private readonly ILogger<GetUserRole> _logger;
        private readonly UserManager<UserAccount> _userManager;
        public GetUserRole(ILogger<GetUserRole> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("GetUserRole")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($" StreamReader GetUserRole :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            if (body != null)
            {
                UserRoleModel urm = null!;
                try
                {
                    urm = JsonConvert.DeserializeObject<UserRoleModel>(body)!;
                }
                catch (Exception ex) { _logger.LogError($" JsonConvert.DeserializeObject<UserRoleModel> :: {ex.Message} "); }

                if (urm != null && !string.IsNullOrEmpty(urm.UserId))
                {
                    try
                    {
                        var userToGet = await _userManager.FindByIdAsync(urm.UserId);
                        if (userToGet == null)
                        {
                            return new NotFoundObjectResult("User not found");
                        }
                        var roles = await _userManager.GetRolesAsync(userToGet);
                        if (roles.Count > 0)
                        {
                            return new OkObjectResult(roles);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error : GetUserRole :: {ex.Message}");
                    }
                    return new NotFoundObjectResult("No role assigned");
                }
            }
            return new BadRequestResult();
        }
    }
}
