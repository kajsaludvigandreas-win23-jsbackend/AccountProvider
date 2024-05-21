using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;

namespace AccountProvider.Functions;

public class SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager)
{
    private readonly ILogger<SignIn> _logger = logger;
    private readonly SignInManager<UserAccount> _signInManager = signInManager;
    private readonly UserManager<UserAccount> _userManager = userManager;

    [Function("SignIn")]
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
            UserLoginReq ulr = null!;
            try
            {
                ulr = JsonConvert.DeserializeObject<UserLoginReq>(body)!;

            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert ULR :: {ex.Message}");
            }
            if (ulr != null && !string.IsNullOrEmpty(ulr.Email) && !string.IsNullOrEmpty(ulr.Password))
            {
                try
                {

                    var user = await _userManager.FindByEmailAsync(ulr.Email);
                    var result = await _signInManager.CheckPasswordSignInAsync(user!, ulr.Password, false);

                   



                    if (result.Succeeded)
                    {
                        if(user!.Email != null && user.Id != null)
                        {
                            TokenRequestModel trm = new TokenRequestModel
                            {
                                Email = user.Email!,
                                UserId = user.Id,
                            };

                        try
                            {
                                trm = JsonConvert.DeserializeObject<TokenRequestModel>(body)!;
                            }

                            catch (Exception ex)
                            {
                                _logger.LogError($"JsonConvert TRM :: {ex.Message}");
                            }


                            using var http = new HttpClient();
                            StringContent content = new StringContent(JsonConvert.SerializeObject(trm), Encoding.UTF8, "application/json");
                            var response = await http.PostAsync("https://tokenprovider-lak.azurewebsites.net/api/token/generate?code=BlxkX3vo2gMEXeGGwUmb3HliUde6gaGeA2FX6GoCp4mgAzFug-fh_A%3D%3D", content);
                            return new OkObjectResult("accessToken");
                        }

                        
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
