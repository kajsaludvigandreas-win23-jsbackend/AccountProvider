using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace AccountProvider.Functions
{
    public class SignInAdmin
    {
        private readonly ILogger<SignInAdmin> _logger;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly UserManager<UserAccount> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SignInAdmin(ILogger<SignInAdmin> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
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
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
                    return new BadRequestResult();
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
                                // Ensure the "Admin" role exists
                                if (!await _roleManager.RoleExistsAsync("Admin"))
                                {
                                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                                    if (!roleResult.Succeeded)
                                    {
                                        _logger.LogError("Failed to create Admin role.");
                                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                                    }
                                }

                                var roles = await _userManager.GetRolesAsync(user);
                                if (!roles.Contains("Admin"))
                                {
                                    var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                                    if (!roleResult.Succeeded)
                                    {
                                        _logger.LogError("Failed to add Admin role to the user.");
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
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }
                }
            }
            return new BadRequestResult();
        }
    }
}
