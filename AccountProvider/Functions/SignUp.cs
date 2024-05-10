using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AccountProvider.Functions;

public class SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)
{
    private readonly ILogger<SignUp> _logger = logger;
    private readonly UserManager<UserAccount> _userManager = userManager;

    [Function("SignUp")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
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
            UserRegRequest urr = null!;
            try
            {
                urr = JsonConvert.DeserializeObject<UserRegRequest>(body)!;

            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert :: {ex.Message}");
            }
            

            if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password))
            {
                if (!await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                {
                    var userAccount = new UserAccount
                    {
                        Email = urr.Email,
                        FirstName = urr.FirstName,
                        LastName = urr.LastName,
                        UserName = urr.Email,
                    };

                    try
                    {
                        var result = await _userManager.CreateAsync(userAccount, urr.Password);

                        if (result.Succeeded)
                        {
                            //get VerificationKey from VerifitionProvider
                            try
                            {
                                using var http = new HttpClient();
                                StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email= userAccount.Email}), Encoding.UTF8 , "application/json");
                                var response = await http.PostAsync("https://siliconvp.azurewebsites.net", content);
                            }
                            catch
                            {

                            }


                            return new OkResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"CreateAsync :: {ex.Message}");
                    }


                }
                else
                {
                    return new ConflictResult();
                }
            }


        }
        return new BadRequestResult();


    }
}
