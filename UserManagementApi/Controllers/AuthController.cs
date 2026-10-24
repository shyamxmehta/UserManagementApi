using UserManagementApi.Modules.Authentication.DTOs;
using UserManagementApi.Modules.Authentication.Services;
using UserManagementApi.Shared.Helpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using System.Security.Claims;

namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AuthService authService, ISessionService _session) : ControllerBase
    {
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginRequestDTO user)
        {
            try {
                var result = await authService.LoginAsync(user);
                if (result is null)
                {
                    return BadRequest("Username or Password is incorrect.");
                }

                var cookieOptions = new CookieOptions
                {
                    // Essential: Prevents client-side scripts from accessing the cookie
                    HttpOnly = true,

                    // Ensures the cookie is only sent over encrypted (HTTPS) connections
                    Secure = true,

                    // Prevents the browser from sending this cookie along with cross-site requests
                    // SameSiteMode.Strict is the most secure; Lax is the default in modern browsers
                    SameSite = SameSiteMode.Lax,

                    // Sets the lifespan of the cookie
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                };

                Response.Cookies.Append("SessionToken", result.Token, cookieOptions);

                await _session.CreateSessionAsync(user.Username, result.Token);

                return Ok(result);
            }
            catch (Exception ex)
            {
                //b errors
                throw new Exception($"{ex.Message}",ex);
            }
            finally
            {
                //dispose resourses created with 'new'
                //return null;
            }
            
            //test to validate dto
            //var validationResult = new List<ValidationResult>();
            //var isValid = Validator.TryValidateObject(user,
            //    new ValidationContext(user), validationResult, validateAllProperties: true);
            //Console.WriteLine(validationResult);
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                //Secure = true,
                SameSite = SameSiteMode.Lax,
            };

            Response.Cookies.Delete("SessionToken", cookieOptions);
            Response.Cookies.Delete(".AspNetCore.Antiforgery", cookieOptions);
            cookieOptions.HttpOnly = false;
            Response.Cookies.Delete("XSRF-TOKEN", cookieOptions);

            //remove other cookies and revoke session here from sessions service
            return Ok("Logged out!");
        }
    }
}
