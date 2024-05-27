using AccountProvider.Models;
using AccountProvider.Services;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions
{
    public class AdminSignIn
    {
        private readonly ILogger<AdminSignIn> _logger;
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly GenerateToken _generateToken;

        public AdminSignIn(ILogger<AdminSignIn> logger, UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, GenerateToken generateToken)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _generateToken = generateToken;
        }

        [Function("AdminSignIn")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "api/admin/signin")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"StreamReader AdminSignIn :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            if (body != null)
            {
                UserSignInModel usim = null!;
                try
                {
                    usim = JsonConvert.DeserializeObject<UserSignInModel>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserSignInModel>//Admin :: {ex.Message}");
                    return new BadRequestObjectResult("Invalid request body");
                }

                if (usim != null && !string.IsNullOrEmpty(usim.Email) && !string.IsNullOrEmpty(usim.Password))
                {
                    try
                    {
                        var userAccount = await _userManager.FindByNameAsync(usim.Email);
                        if (userAccount == null)
                        {
                            return new UnauthorizedResult();
                        }

                        var res = await _signInManager.CheckPasswordSignInAsync(userAccount, usim.Password, false);
                        if (res.Succeeded)
                        {
                            var roles = await _userManager.GetRolesAsync(userAccount);
                            if (roles.Contains("admin"))
                            {
                                var tokenResponse = await _generateToken.GenerateTokenAsync(usim.Email, userAccount.Id);
                                if (tokenResponse.IsSuccessStatusCode)
                                {
                                    var token = await tokenResponse.Content.ReadAsStringAsync();
                                    return new OkObjectResult(token);
                                }
                                else
                                {
                                    _logger.LogError($"TokenProvider Response :: {tokenResponse.StatusCode}");
                                    return new StatusCodeResult((int)tokenResponse.StatusCode);
                                }
                            }
                            else
                            {
                                return new ForbidResult();
                            }
                        }
                        else
                        {
                            return new UnauthorizedResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Admin SignIn :: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            return new BadRequestResult();
        }
    }
}
