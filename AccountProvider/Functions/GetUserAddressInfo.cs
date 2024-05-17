using AccountProvider.Models;
using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions
{
    public class GetUserAddressInfo
    {
        private readonly ILogger<GetUserAddressInfo> _logger;
        private readonly DataContext _dataContext;
        public GetUserAddressInfo(ILogger<GetUserAddressInfo> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }

        [Function("GetUserAddressInfo")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
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
                UserAddressModel uam = null!;
                try
                {
                    uam = JsonConvert.DeserializeObject<UserAddressModel>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($" JsonConvert.DeserializeObject<UserAddressModel/Get> :: {ex.Message} ");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                if (uam != null && !string.IsNullOrEmpty(uam.UserId))
                {
                    try
                    {
                        var addressInfo = await _dataContext.UserAddress.FirstOrDefaultAsync(x => x.UserId == uam.UserId);
                        if (addressInfo != null)
                        {
                            var json = JsonConvert.SerializeObject(addressInfo);
                            return new OkObjectResult(json);
                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($" Get User Address Info :: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                }
            }
            return new BadRequestResult();
        }
    }
}
