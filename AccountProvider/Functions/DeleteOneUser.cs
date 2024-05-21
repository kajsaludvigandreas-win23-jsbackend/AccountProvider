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
    public class DeleteOneUser
    {
        private readonly ILogger<DeleteOneUser> _logger;
        private readonly UserManager<UserAccount> _userManager;

        public DeleteOneUser(ILogger<DeleteOneUser> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("DeleteOneUser")]
        public async Task <ActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();
                DeleteOneUserModel? userModel = JsonConvert.DeserializeObject<DeleteOneUserModel>(body);

                if (string.IsNullOrEmpty(userModel?.Email))
                {
                    return new BadRequestObjectResult("No email provided");
                }

                UserAccount? user = _userManager.FindByEmailAsync(userModel.Email).Result;

                if (user == null)
                {
                    return new BadRequestObjectResult("User not found");
                }

                IdentityResult result = _userManager.DeleteAsync(user).Result;

                if (result.Succeeded)
                {
                    return new OkObjectResult("User deleted");
                }
                else
                {
                    return new BadRequestObjectResult("User could not be deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteOneUser :: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }            
        }
    }
}
