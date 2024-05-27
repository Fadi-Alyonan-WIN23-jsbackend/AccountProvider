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

public class UserRoleUpdate
{
    private readonly ILogger<UserRoleUpdate> _logger;
    private readonly UserManager<UserAccount> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;


    public UserRoleUpdate(ILogger<UserRoleUpdate> logger, UserManager<UserAccount> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Function("UserRoleUpdate")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;
        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($" StreamReader.UpdateUserRoles :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        if (body != null)
        {
            UserRoleModel urm = null!;
            try
            {
                urm = JsonConvert.DeserializeObject<UserRoleModel>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($" JsonConvert.DeserializeObject<UserRoleModel> :: {ex.Message} ");
                return new BadRequestObjectResult("Invalid request body");
            }

            if (urm != null && !string.IsNullOrEmpty(urm.UserId) && (urm.Role == "user" || urm.Role == "admin"))
            {
                var userToUpdate = await _userManager.FindByIdAsync(urm.UserId);
                if (userToUpdate == null)
                {
                    return new NotFoundObjectResult("User not found");
                }

                try
                {
                    var currentRoles = await _userManager.GetRolesAsync(userToUpdate);

                    if (currentRoles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(userToUpdate, currentRoles);
                    }

                    var roleExists = await _roleManager.RoleExistsAsync(urm.Role);
                    if (!roleExists)
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole(urm.Role));
                        if (!roleResult.Succeeded)
                        {
                            _logger.LogError($"Error : UserRoleUpdate :: Failed to create role {urm.Role}");
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        }
                    }

                    await _userManager.AddToRoleAsync(userToUpdate, urm.Role);

                    return new OkObjectResult(new { Success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error : UserRoleUpdate :: {ex.Message}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
        }
        return new BadRequestResult();
    }
}