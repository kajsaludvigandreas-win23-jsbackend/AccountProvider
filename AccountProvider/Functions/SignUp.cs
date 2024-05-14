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
using Azure.Messaging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Azure.Messaging.ServiceBus;



namespace AccountProvider.Functions;

public class SignUp
{
    private readonly ILogger<SignUp> _logger;
    private readonly UserManager<UserAccount> _userManager;
    private readonly QueueClient _queueClient;

    public SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)
    {
        _logger = logger;
        _userManager = userManager;
        string serviceBusConnection = Environment.GetEnvironmentVariable("ServiceBusConnection")!;
        string queueName = Environment.GetEnvironmentVariable("ServiceBusQueueName")!;
        _queueClient = new QueueClient(serviceBusConnection, queueName);
    }

    [Function("SignUp")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req )

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

                    var emailForVerify = new EmailModel
                    {
                        Email = urr.Email,
                    };

                    try
                    {
                        var result = await _userManager.CreateAsync(userAccount, urr.Password);

                        if (result.Succeeded)
                        {
                            //get VerificationKey from VerifitionProvider
                            try
                            {
                               
                                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EmailModel { Email = urr.Email })));

                                var queueClient = new QueueClient("Endpoint=sb://siliconservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=fDe1UrUGMYN+1C/isNoqM+QYTS0nzkbeK+ASbDxsCSE=", "verification_request");
                                await queueClient.SendAsync(message);


                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"verifcationtrigger :: {ex.Message}");
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


    public class EmailModel
    {
        public string Email { get; set; } = null!;
    }
}

