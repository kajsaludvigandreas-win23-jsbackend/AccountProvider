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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
       
        
        
        string body = await new StreamReader(req.Body).ReadToEndAsync();
        UserRegRequest urr = JsonConvert.DeserializeObject<UserRegRequest>(body)!;

        if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password))
        {
            if (!await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
            {
                var userAccount = new UserAccount
                {
                    Email = urr.Email,
                    FirstName = urr.FirstName,
                    LastName = urr.LastName,
                    UserName = urr.Email
                };

                var emailForVerify = new EmailForVerify
                {
                    Email = urr.Email
                };

                var result = await _userManager.CreateAsync(userAccount, urr.Password);
                if (result.Succeeded)
                {



                    var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailForVerify)));
                    await _queueClient.SendAsync(message);
                    return new OkResult();
                }
            }
            else
            {
                return new ConflictResult();
            }
        }
        return new BadRequestResult();
    }

    public class EmailForVerify
    {
        public string Email { get; set; } = null!;
    }
}

