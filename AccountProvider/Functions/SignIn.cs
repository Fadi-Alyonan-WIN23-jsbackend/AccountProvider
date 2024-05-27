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
    public class SignIn
    {
        private readonly ILogger<SignIn> _logger;
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly GenerateToken _generateToken;
        public SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager, GenerateToken generateToken)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _generateToken = generateToken;
        }

        [Function("SignIn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex) { _logger.LogError($" StreamReader SignIn :: {ex.Message}"); }

            if (body != null)
            {
                UserSignInModel usim = null!;
                try
                {
                    usim = JsonConvert.DeserializeObject<UserSignInModel>(body)!;
                }
                catch (Exception ex) { _logger.LogError($" JsonConvert.DeserializeObject<UserSignInModel> :: {ex.Message} "); }

                if (usim != null && !string.IsNullOrEmpty(usim.Email) && !string.IsNullOrEmpty(usim.Password))
                {

                    try
                    {
                        var userAccount = await _userManager.FindByNameAsync(usim.Email);
                        var res = await _signInManager.CheckPasswordSignInAsync(userAccount!, usim.Password, false);
                        if (res.Succeeded)
                        {
                            var tokenResponse = await _generateToken.GenerateTokenAsync(usim.Email, userAccount.Id);
                            if (tokenResponse.IsSuccessStatusCode)
                            {
                                var token = await tokenResponse.Content.ReadAsStringAsync();
                                return new OkObjectResult(token);
                            }
                            else
                            {
                                _logger.LogError($" TokenProvider Response :: {tokenResponse.StatusCode}");
                                return new StatusCodeResult((int)tokenResponse.StatusCode);
                            }
                        }
                        else
                        {
                            return new UnauthorizedResult();
                        }
                    }
                    catch (Exception ex) { _logger.LogError($" User SignIn Manager :: {ex.Message}"); }

                }
            }
            return new BadRequestResult();
        }
    }
}
