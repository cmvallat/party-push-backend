using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Queries;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Amazon;
using Amazon.Runtime;
using Amazon.RDSDataService;
using Amazon.RDSDataService.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Core.Commands;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Common;

namespace Api.LoginController
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private IMediator _mediator; 
        public LoginController(IConfiguration config, IMediator mediator)
        {
            _config = config;
            _mediator = mediator;
        }

        // [AllowAnonymous]
        // [HttpPost("authenticate-user-and-get-JWT")]
        // public async Task<IActionResult> GetJWT([FromBody] UserLogin userLogin)
        // {
        //     var user = Authenticate(userLogin).Result;
        //     if (user != null)
        //     {
        //         var token = GenerateToken(user);
        //         return StatusCode(200, new { message = token });
        //     }
        //     return StatusCode(404, new { message = "User not found" });
        // }

        // // To generate token
        // private string GenerateToken(UserModel user)
        // {
        //     var key = _config["Jwt:Key"];
        //     var issuer = _config["Jwt:Issuer"];
        //     var audience = _config["Jwt:Audience"];

        //     var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6xGrJLpenwmJXCIAlLPbdfbgctrbgvrtgcrtdbgvgrdffvxrsvfdfrcvftryr65gr4sdrger"));
        //     var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //     var claims = new[]
        //     {
        //         new Claim(ClaimTypes.NameIdentifier,user.Username),
        //         new Claim(ClaimTypes.Role,user.Role),
        //     };
        //     var token = new JwtSecurityToken(
        //         "https://localhost:5001/",
        //         "https://localhost:5001/",
        //         claims,
        //         expires: DateTime.Now.AddMinutes(15),
        //         signingCredentials: credentials);


        //     return new JwtSecurityTokenHandler().WriteToken(token);

        // }

        // //To authenticate user
        // private async Task<UserModel> Authenticate(UserLogin userLogin)
        // {
        //     // var currentUser = UserConstants.Users.FirstOrDefault(
        //     //     x => x.Username.ToLower() == userLogin.Username.ToLower() 
        //     //     && x.Password == userLogin.Password);

        //     var currentUser = await _mediator.Send(new UsersQuery.Query() {
        //             Username = userLogin.Username, 
        //             Password = userLogin.Password, 
        //         });

        //     if (currentUser != null)
        //     {
        //         return new UserModel
        //         {
        //             Username = currentUser.username,
        //             Password = currentUser.password,
        //             Role = "Validated",
        //         };
        //     }
        //     return null;
        // }
    }
}