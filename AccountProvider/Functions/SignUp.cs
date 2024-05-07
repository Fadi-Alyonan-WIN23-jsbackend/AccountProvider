using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace AccountProvider.Functions
{
    public class SignUp
    {
        private readonly ILogger<SignUp> _logger;
        private readonly UserManager<UserAccount> _userManager;
        public SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("SignUp")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                string body = null!;
                try
                {
                    body = await new StreamReader(req.Body).ReadToEndAsync();
                }
                catch (Exception ex) { _logger.LogError($" StreamReader :: {ex.Message}"); }
                
                if (body != null)
                {
                    UserRegistrationModel urm = null!;
                    try
                    {
                        urm = JsonConvert.DeserializeObject<UserRegistrationModel>(body)!;
                    }
                    catch (Exception ex) { _logger.LogError($" JsonConvert.DeserializeObject<UserRegistrationModel> :: {ex.Message} "); }

                    if (urm != null && !string.IsNullOrEmpty(urm.Email) && !string.IsNullOrEmpty(urm.Password) && !string.IsNullOrEmpty(urm.FirstName) && !string.IsNullOrEmpty(urm.LastName))
                    {
                        if (! await _userManager.Users.AnyAsync(x => x.Email == urm.Email))
                        {
                            var userAccount = new UserAccount
                            {
                                FirstName = urm.FirstName,
                                LastName = urm.LastName,
                                Email = urm.Email,
                                UserName = urm.Email,
                            };
                            try
                            {
                                var res = await _userManager.CreateAsync(userAccount, urm.Password);
                                if (res.Succeeded)
                                {
                                    return new OkResult();
                                }
                            }
                            catch (Exception ex) { _logger.LogError($" User Manager Create :: {ex.Message}"); }

                            
                        }else
                        {
                            return new ConflictResult();
                        }
                    }
                }
                return new BadRequestResult();
            }catch (Exception ex) { _logger.LogError(ex.Message); }
            return new BadRequestResult();
        }
    }
}
