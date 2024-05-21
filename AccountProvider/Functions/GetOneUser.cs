using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace AccountProvider.Functions
{
    public class GetOneUser
    {
        private readonly ILogger<GetOneUser> _logger;
        private readonly UserManager<UserAccount> _userManager;

        public GetOneUser(ILogger<GetOneUser> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("GetOneUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            string body = null!;


            try
            {
                body = await new StreamReader(req.Body).ReadToEndAsync();

            }
            catch (Exception ex)
            {

                _logger.LogError($"StreamReader :: {ex.Message}");
            }

            if (body != null)
            {
                GetOneUserModel userModel = null!;

                try
                {
                    userModel = JsonConvert.DeserializeObject<GetOneUserModel>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert :: {ex.Message}");
                }

                if (string.IsNullOrEmpty(userModel.Email))
                {
                    return new BadRequestObjectResult("No email provided");
                }

                UserAccount? user = null!;

                try
                {
                    
                    user = await _userManager.FindByEmailAsync(userModel.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"FindByEmailAsync :: {ex.Message}");
                }

                if (user == null)
                {
                    return new NotFoundObjectResult("User not found");
                }

                return new OkObjectResult(user);
            }
            return new BadRequestObjectResult("No body provided");
        }
    }
}
