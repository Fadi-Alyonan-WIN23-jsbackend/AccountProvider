using AccountProvider.Models;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class UpdateUserInformation
{
    private readonly ILogger<UpdateUserInformation> _logger;
    private readonly UserManager<UserAccount> _userManager;
    private readonly DataContext _dataContext;
    public UpdateUserInformation(ILogger<UpdateUserInformation> logger, UserManager<UserAccount> userManager, DataContext dataContext)
    {
        _logger = logger;
        _userManager = userManager;
        _dataContext = dataContext;
    }

    [Function("UpdateUserInformation")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;
        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
        }
        catch (Exception ex) { _logger.LogError($" StreamReader.UpdateAddress :: {ex.Message}"); }

        if (body != null)
        {
            UserInformationModel uim = null!;
            try
            {
                uim = JsonConvert.DeserializeObject<UserInformationModel>(body)!;
            }
            catch (Exception ex) { _logger.LogError($" JsonConvert.DeserializeObject<UserInformationModel> :: {ex.Message} "); }


            if (uim != null && !string.IsNullOrEmpty(uim.FirstName) && !string.IsNullOrEmpty(uim.LastName) && !string.IsNullOrEmpty(uim.Email))
            {
                var userToUpdate = await _userManager.FindByIdAsync(uim.UserId);
                if (userToUpdate != null)
                {
                    userToUpdate.FirstName = uim.FirstName;
                    userToUpdate.LastName = uim.LastName;
                    userToUpdate.Email = userToUpdate.Email;
                    userToUpdate.PhoneNumber = uim.PhoneNumber;
                    userToUpdate.Bio = uim.Biography;

                    try
                    {
                        await _userManager.UpdateAsync(userToUpdate);
                        await _dataContext.SaveChangesAsync();
                        var json = JsonConvert.SerializeObject(userToUpdate);
                        return new OkObjectResult(json);

                    }
                    catch (Exception ex) { _logger.LogError($" Update user info :: {ex.Message}"); }
                   
                }
            }

        }
        return new BadRequestResult();
    }
}
