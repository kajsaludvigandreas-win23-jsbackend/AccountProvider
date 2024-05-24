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
    public class SignInAdmin
    {
        private readonly ILogger<SignInAdmin> _logger;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly UserManager<UserAccount> _userManager;

        public SignInAdmin(ILogger<SignInAdmin> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Function("SignInAdmin")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;
            try
            {
                using (var reader = new StreamReader(req.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"StreamReader :: {ex.Message}");
            }

            if (body != null)
            {
                UserLoginReq ulr = null!;
                try
                {
                    ulr = JsonConvert.DeserializeObject<UserLoginReq>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert :: {ex.Message}");
                }

                if (ulr != null && !string.IsNullOrEmpty(ulr.Email) && !string.IsNullOrEmpty(ulr.Password))
                {
                    try
                    {
                        var result = await _signInManager.PasswordSignInAsync(ulr.Email, ulr.Password, ulr.IsPersistent, false);

                        if (result.Succeeded)
                        {
                            var user = await _userManager.FindByEmailAsync(ulr.Email);
                            if (user != null)
                            {
                                var roles = await _userManager.GetRolesAsync(user);
                                if (!roles.Contains("Admin"))
                                {
                                    var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                                    if (!roleResult.Succeeded)
                                    {
                                        _logger.LogError("Failed to add admin role to the user.");
                                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                    }
                                }
                       
                                return new OkObjectResult("accesstoken");
                            }
                            return new NotFoundResult();
                        }
                        return new UnauthorizedResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"PasswordSignInAsync :: {ex.Message}");
                    }
                }
            }
            return new BadRequestResult();
        }
    }
}
