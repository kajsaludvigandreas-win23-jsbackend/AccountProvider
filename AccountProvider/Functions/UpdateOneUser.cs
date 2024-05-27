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
    public class UpdateOneUser
    {
        private readonly ILogger<UpdateOneUser> _logger;
        private readonly UserManager<UserAccount> _userManager;

        public UpdateOneUser(ILogger<UpdateOneUser> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("UpdateOneUser")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequest req)
        {

            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();
                UpdateOneUserModel? userModel = JsonConvert.DeserializeObject<UpdateOneUserModel>(body);

                if (string.IsNullOrEmpty(userModel?.Email))
                {
                    return new BadRequestObjectResult("No email provided");
                }

                UserAccount? user = _userManager.FindByEmailAsync(userModel.Email).Result;

                if (user == null)
                {
                    return new BadRequestObjectResult("User not found");
                }

                if (!string.IsNullOrEmpty(userModel.NewEmail))
                {
                    user.Email = userModel.NewEmail;
                }

                if (!string.IsNullOrEmpty(userModel.NewPassword))
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userModel.NewPassword);
                }

                if (!string.IsNullOrEmpty(userModel.FirstName))
                {
                    user.FirstName = userModel.FirstName;
                }

                if (!string.IsNullOrEmpty(userModel.LastName))
                {
                    user.LastName = userModel.LastName;
                }

                if (!string.IsNullOrEmpty(userModel.PhoneNumber))
                {
                    user.PhoneNumber = userModel.PhoneNumber;
                }

                if(!string.IsNullOrEmpty(userModel.ProfileImageUri)) 
                {
                    user.ProfileImageUri = userModel.ProfileImageUri;
                }

                IdentityResult result = _userManager.UpdateAsync(user).Result;

                if (result.Succeeded)
                {
                    
                    return new OkObjectResult("User updated");
                }
                else
                {
                    return new BadRequestObjectResult("User could not be updated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateOneUser :: {ex.Message}");
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
