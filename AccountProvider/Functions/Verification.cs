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
    public class Verification
    {
        private readonly ILogger<Verification> _logger;
        private readonly UserManager<UserAccount> _userManager;

        public Verification(ILogger<Verification> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("Verification")]
        public async Task <IActionResult>Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();
            }
            catch (Exception ex) { _logger.LogError($" StreamReader Verification :: {ex.Message}"); }

            if (body != null)
            {
                VerificationModel vm = null!;
                try
                {
                    vm = JsonConvert.DeserializeObject<VerificationModel>(body)!;
                }
                catch (Exception ex) { _logger.LogError($" JsonConvert.DeserializeObject<VerificationModel> :: {ex.Message} "); }

                if (vm != null && !string.IsNullOrEmpty(vm.Email) && !string.IsNullOrEmpty(vm.VerificationCode))
                {

                    try
                    {
                        var res = true;
                        if (res)
                        {
                            var userAccount = await _userManager.FindByEmailAsync(vm.Email);
                            if (userAccount != null)
                            {
                                userAccount.Email = vm.VerificationCode;
                                await _userManager.UpdateAsync(userAccount);
                                if(await _userManager.IsEmailConfirmedAsync(userAccount))
                                {
                                    return new OkResult();
                                }
                            }
                        }
                    }
                    catch (Exception ex) { _logger.LogError($" Verification :: {ex.Message}"); }

                }
            }
            return new UnauthorizedResult();
        }
    }
}
