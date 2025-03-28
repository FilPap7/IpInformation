using Common.Cache;
using DataAccess.Data;
using IpInformation.Helpers;
using ApiModels.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace IpInformation.Controllers
{
    public class LoginController : ControllerBase
    {
        private readonly IpInformationDbContext _dbContext;

        public LoginController(IpInformationDbContext context)
        {
            _dbContext = context;
        }


        [HttpPost]
        [Route("Ip/Login")]
        public async Task<IActionResult> ValidateLoginAsync([FromBody] Credentials credentials)
        {

            if (!ModelState.IsValid) Redirect("pages/Accounts/AccessDenied.html");

            if (!DatabaseValidation.ValidateCredentials(credentials))
                throw new Exception("Invalid Username or Password");

            //Create the Security Context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@test.com")
            };

            var identity = new ClaimsIdentity(claims, Constants.DefaultCookie);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);


            try
            {
                await HttpContext.SignInAsync(Constants.DefaultCookie, claimsPrincipal);
            }
            catch (Exception e)
            {
                return Redirect("/pages/Accounts/AccessDenied.html");
            }

            return Redirect("/index.html");
        }
    }
}
