using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace IntuneAdminApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminUsersController : ControllerBase
    {
        private readonly GraphServiceClient _graphClient;

        public AdminUsersController()
        {
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var clientId = MyConfig.GetValue<string>("AzureAd:ClientId");
            var clientSecret = MyConfig.GetValue<string>("AzureAd:ClientSecret");

            //var scopes = new[] { "https://graph.microsoft.com/.default" };
            var scopes = new[] { 
                "https://graph.microsoft.com/User.ReadWrite.All", 
                "https://graph.microsoft.com/Directory.ReadWrite.All", 
                "https://graph.microsoft.com/RoleManagement.ReadWrite.Directory" };

            var tenantId = MyConfig.GetValue<string>("AzureAd:TenantId");

            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret);

            _graphClient = new GraphServiceClient(clientSecretCredential, scopes);
        }

        [HttpGet]
        public async Task<IActionResult> AddAdminUser([FromQuery] string userPrincipalName)
        {
            try
            {
                var adminUser = new User
                {
                    UserPrincipalName = userPrincipalName,
                    DisplayName = "Admin User",
                    MailNickname = "adminuser",
                    UserType = "Member",
                    AccountEnabled = true,
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = true,
                        Password = "YourSecurePassword123!"
                    }
                };

                var newUser = await _graphClient.Users
                    .Request()
                    .AddAsync(adminUser);

                // Assign the admin role
                /*var roleAssignment = new DirectoryRole
                {
                    RoleTemplateId = "role-template-id", // Replace with the actual role template ID for Intune Administrator
                    Members = new DirectoryObject[]
                    {
                        new DirectoryObject { Id = newUser.Id }
                    }
                };

                await _graphClient.DirectoryRoles
                    .Request()
                    .AddAsync(roleAssignment);*/

                return Ok(newUser);
            }
            catch (ServiceException ex)
            {
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
        }
    }
}
