using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Queries;
using Mediator;

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

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin).Result;
            if (user != null)
            {
                var token = GenerateToken(user);
                return Ok(token);
            }

            return NotFound("user not found");
        }

        // To generate token
        private string GenerateToken(UserModel user)
        {
            var key = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6xGrJLpenwmJXCIAlLPbdfbgctrbgvrtgcrtdbgvgrdffvxrsvfdfrcvftryr65gr4sdrger"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Username),
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.UserData,user.party_code)
            };
            var token = new JwtSecurityToken(
                "https://localhost:5001/",
                "https://localhost:5001/",
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        //To authenticate user
        private async Task<UserModel> Authenticate(UserLogin userLogin)
        {
            // var currentUser = UserConstants.Users.FirstOrDefault(
            //     x => x.Username.ToLower() == userLogin.Username.ToLower() 
            //     && x.Password == userLogin.Password);

            var currentUser = await _mediator.Send(new GuestQuery.Query() {Guest_name = userLogin.Username, Party_code = userLogin.party_code});

            if (currentUser != null)
            {
                return new UserModel{
                    Username = currentUser.guest_name,
                    Role = "Admin",
                    guest_name = currentUser.guest_name,
                    party_code = currentUser.party_code,
                    at_party = currentUser.at_party
                };
            }
            return null;
        }
    }
}