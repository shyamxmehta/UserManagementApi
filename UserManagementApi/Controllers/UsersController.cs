using UserManagementApi.Data.Seeders;
using UserManagementApi.Modules.Users.DTOs;
using UserManagementApi.Modules.Users.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(
        IAntiforgery _antiforgery, 
        UsersService usersService) : ControllerBase
    {

        [HttpPost("register-user")]
        public async Task<ActionResult> RegisterUser(AddUserRequestDTO user)
        {
            var result = await usersService.RegisterUser<int?>(user);
            if (result == -1)
                return BadRequest("User Exists");

            return Ok(result);
        }

        [HttpGet("view-users")]
        public async Task<ActionResult> GetUsers()
        {
            var users = await usersService.GetAllUsers();
            return Ok(users);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("check-login")]
        public async Task<ActionResult> CheckLogin()
        {
            return Ok("I am Authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("check-admin")]
        public async Task<ActionResult> CheckAdmin()
        {
            return Ok("I Am Admin!");
        } 

        [HttpGet("xsrf-token")]
        public ActionResult GetToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
            {
                HttpOnly = false,
                //Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return Ok(new { token = tokens.RequestToken });
        }

        //[Authorize(Roles = "admin")]
        //[HttpPost("decrypt-token")]
        //public async Task<ActionResult> DecryptToken([FromQuery] string Token)
        //{
        //    if (String.IsNullOrEmpty(Token))
        //    {
        //        return Ok("No token provided");
        //    }

        //    var decryptedString = await usersService.DecryptToken(Token);

        //    return Ok(decryptedString);
        //}

        [Authorize(Policy = Permissions.UpdateUser)]
        [HttpPut("give-delete-rights")]
        public async Task<ActionResult> GiveUserDeleteRights()
        {
            var result = await usersService.GiveDeleteRights();

            if(result == false)
            {
                return BadRequest("Unable to insert");
            }

            return Ok("Rights given!");
        }

        [Authorize(Policy = Permissions.DeleteUser)]
        [HttpDelete("delete-user")]
        public async Task<ActionResult> DeleteUser()
        {
            return Ok("Delete user allowed!");
        }
    }
}
