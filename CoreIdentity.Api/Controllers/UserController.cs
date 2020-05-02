using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CoreIdentity.Api.Identity;
using CoreIdentity.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CoreIdentity.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private  UserManager<ApplicationUser> _userManager;
        private RoleManager<ApplicationRole> _roleManager;
        private SignInManager<ApplicationUser> _signInManager;

        public UserController(UserManager<ApplicationUser> userManager,RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> UserCreate([FromBody]UserCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = new ApplicationUser
                {
                    UserName = model.UserName.Trim(),
                    Email = model.Email.Trim()
                };
                IdentityResult result = await _userManager.CreateAsync(applicationUser, model.Password.Trim());
                if (result.Succeeded)
                {

                }
                return Ok();
            }
            return BadRequest("Bilgileriniz hatalı");
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);
                if (user!=null && await _userManager.CheckPasswordAsync(user,model.Password.Trim()))
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                    };

                    var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecureKey"));

                    var token = new JwtSecurityToken(

                        issuer: "http://google.com",
                        audience: "http://google.com",
                        claims: claims,
                        signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token),expiration = token.ValidTo });

                }

                return Ok();
            }
            return BadRequest("Please check your requests");
        }


    }
}