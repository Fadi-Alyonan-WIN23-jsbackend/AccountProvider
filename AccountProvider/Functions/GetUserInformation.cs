using AccountProvider.Models;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions
{
    public class GetUserInformation
    {
        private readonly ILogger<GetUserInformation> _logger;
        private readonly UserManager<UserAccount> _userManager;
        public GetUserInformation(ILogger<GetUserInformation> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("GetUserInformation")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($" StreamReader GetUserAddressInfo :: {ex.Message}");
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
                    _logger.LogError($" JsonConvert.DeserializeObject<UserAddressModel/Get> :: {ex.Message} ");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                if (uim != null && !string.IsNullOrEmpty(uim.UserId))
                {
                    try
                    {
                        var userInfo = await _userManager.FindByIdAsync(uim.UserId);
                        if (userInfo != null)
                        {
                            var json = JsonConvert.SerializeObject(userInfo);
                            return new OkObjectResult(json);
                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($" Get User Info :: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                }
            }
            return new BadRequestResult();
        }
    }
}
