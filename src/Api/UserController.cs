using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.UserController
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // [HttpGet("validate-user")]
        // // [Route("Admins")]
        // [Authorize(Roles = "Validated")]
        // public string AdminEndPoint()
        // {
        //     var currentUser = GetCurrentUser();
        //     return currentUser.Username;
        // }
        // private UserModel GetCurrentUser()
        // {
        //     var identity = HttpContext.User.Identity as ClaimsIdentity;
        //     if (identity != null)
        //     {
        //         var userClaims = identity.Claims;
        //         return new UserModel
        //         {
        //             Username = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
        //             Role = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
        //         };
        //     }
        //     return null;
        // }
    }
}