using AccountProvider.Models;
using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class UpdateAddress
{
    private readonly UserManager<UserAccount> _userManager;
    private readonly ILogger<UpdateAddress> _logger;
    private readonly DataContext _dataContext;
    public UpdateAddress(ILogger<UpdateAddress> logger, DataContext dataContext, UserManager<UserAccount> userManager)
    {
        _logger = logger;
        _dataContext = dataContext;
        _userManager = userManager;
    }

    [Function("UpdateAddress")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;
        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
        }
        catch (Exception ex) { _logger.LogError($" StreamReader.UpdateAddress :: {ex.Message}"); }

        if (body != null)
        {
            UserAddressModel uam = null!;
            try
            {
                uam = JsonConvert.DeserializeObject<UserAddressModel>(body)!;
            }
            catch (Exception ex) { _logger.LogError($" JsonConvert.DeserializeObject<UserAddressModel> :: {ex.Message} "); }


            if (uam != null)
            {
                var user = await _userManager.FindByIdAsync(uam.UserId);
                if (user != null)
                {
                    var existingUserAddress = await _dataContext.UserAddress.FirstOrDefaultAsync(x => x.UserId == uam.UserId);
                    if (existingUserAddress != null)
                    {

                        existingUserAddress.AddressLine1 = uam.AddressLine1;
                        existingUserAddress.AddressLine2 = uam.AddressLine2;
                        existingUserAddress.PostalCode = uam.PostalCode;
                        existingUserAddress.City = uam.City;

                        try
                        {
                            _dataContext.UserAddress.Entry(existingUserAddress).CurrentValues.SetValues(uam);
                            await _dataContext.SaveChangesAsync();
                            var json = JsonConvert.SerializeObject(existingUserAddress);
                            return new OkObjectResult(json);

                        }
                        catch (Exception ex) { _logger.LogError($" Update address info :: {ex.Message}"); }
                    } else if (existingUserAddress == null)
                    {
                        var newUserAddress = new UserAddress
                        {
                            UserId = uam.UserId,
                            AddressLine1 = uam.AddressLine1,
                            AddressLine2 = uam.AddressLine2,
                            PostalCode = uam.PostalCode,
                            City = uam.City
                        };
                    try
                        {
                            _dataContext.UserAddress.Add(newUserAddress);
                            await _dataContext.SaveChangesAsync();
                            var json = JsonConvert.SerializeObject(newUserAddress);
                            return new OkObjectResult(json);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($" UpdateAddress. create address info :: {ex.Message}");
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        }
                    }
                }
            }
            
        }
        return new BadRequestResult();
    }
}

