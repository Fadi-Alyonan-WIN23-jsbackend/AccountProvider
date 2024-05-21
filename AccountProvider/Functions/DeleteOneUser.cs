using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions
{
    public class DeleteOneUser
    {
        private readonly ILogger<DeleteOneUser> _logger;
        private readonly UserManager<UserAccount> _userManager;

        public DeleteOneUser(ILogger<DeleteOneUser> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("DeleteOneUser")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"StreamReader DeleteUser :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            if (body != null)
            {
                UserInformationModel uim = null!;
                try
                {
                    uim = JsonConvert.DeserializeObject<UserInformationModel>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserInformationModel/Delete> :: {ex.Message}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                if (uim != null && !string.IsNullOrEmpty(uim.UserId))
                {
                    try
                    {
                        var user = await _userManager.FindByIdAsync(uim.UserId);
                        if (user != null)
                        {
                            var result = await _userManager.DeleteAsync(user);
                            if (result.Succeeded)
                            {
                                return new OkResult();
                            }
                            else
                            {
                                _logger.LogError($"User deletion failed :: ");
                                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                            }
                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Delete User :: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            return new BadRequestResult();
        }
    }
}
