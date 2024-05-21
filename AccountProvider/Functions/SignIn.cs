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

public class SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager)
{
    private readonly ILogger<SignIn> _logger = logger;
    private readonly SignInManager<UserAccount> _signInManager = signInManager;

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
                _logger.LogError($"JsonConvert :: {ex.Message}");
            }
            if (ulr != null && !string.IsNullOrEmpty(ulr.Email) && !string.IsNullOrEmpty(ulr.Password))
            {
                try
                {
                    var result = await _signInManager.PasswordSignInAsync(ulr.Email, ulr.Password, ulr.IsPersistent, false);

                    if(result.Succeeded)
                    {
                        //TokenRequestModel trm = null!;

                        //var user = await _userManager.FindByEmailAsync(ulr.Email);

                        //try
                        //{
                        //    trm = JsonConvert.DeserializeObject<TokenRequestModel>(body)
                        //}
                        //catch
                        //{

                        //}

                        //using var http = new HttpClient();
                        //StringContent content = new StringContent(JsonConvert.SerializeObject(ulr), Encoding.UTF8, "application/json");
                        //var response = await http.PostAsync("https://tokenprovider-lak.azurewebsites.net/api/token/generate?code=BlxkX3vo2gMEXeGGwUmb3HliUde6gaGeA2FX6GoCp4mgAzFug-fh_A%3D%3D", content);
                        return new OkResult();
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
