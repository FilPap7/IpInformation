﻿using Common.Cache;
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
        private readonly ICacheService _cacheContext;

        public LoginController(IpInformationDbContext context, ICacheService cache)
        {
            _dbContext = context;
            _cacheContext = cache;
        }


        [HttpPost]
        [Route("ValidateLogin")]
        public async Task<IActionResult> ValidateLoginAsync([FromBody] Credentials credentials)
        {

            if (!ModelState.IsValid) throw new Exception("Model State is not valid!");

            if (credentials.UserName.IsNullOrEmpty() || credentials.Password.IsNullOrEmpty())
                throw new Exception("Username or Password cannot be empty");

            //Create the security Context
            if (!DatabaseValidation.ValidateCredentials(credentials))
                throw new Exception("Invalid Username or Password");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Email, "admin@mywebsite.com")
            };

            var identity = new ClaimsIdentity(claims, "MyCookie");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

            return RedirectToPage("/Index");
        }
    }
}
