using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AccountProvider.Functions
{
    public class GetAllUsers
    {
        private readonly ILogger<GetAllUsers> _logger;
        private readonly UserManager<UserAccount> _userManager;

        public GetAllUsers(ILogger<GetAllUsers> logger, UserManager<UserAccount> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Function("GetAllUsers")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            try
            {
                var users = _userManager.Users;
                return new OkObjectResult(users);

            }
            catch (Exception ex)
            {
                
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
