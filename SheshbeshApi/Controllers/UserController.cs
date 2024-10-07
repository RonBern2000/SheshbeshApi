using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SheshbeshApi.DAL;
using SheshbeshApi.Filters;
using SheshbeshApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SheshbeshApi.Controllers
{
    [ServiceFilter(typeof(ActionsFilter))]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _usersService;
        private readonly JwtSettings _jwtSettings;
        public UserController(IUserRepository usersService, IOptions<JwtSettings> jwtSettings)
        {
            _usersService = usersService;
            _usersService = usersService;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpGet("allUsers")]
        public async Task<ActionResult<List<ResponseUser>>> GetAllUsers()
        {
            var responseUsers = await _usersService.GetAsync();
            return Ok(responseUsers);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            var user = await _usersService.GetAsync(id);

            if (user is null)
                return NotFound();

            var resUser = new ResponseUser { Id = user.Id, Username = user.UserName, Email = user.Email };

            return Ok(resUser);
        }

        [HttpPost("authStatus")]
        [Authorize]
        public async Task<IActionResult> CheckAuthStatus()
        {
            var userName = User.Identity!.Name;
            var user = await _usersService.GetUserByUsernameAsync(userName!);
            var resUser = new ResponseUser { Id = user!.Id, Username = user.UserName, Email = user.Email };
            return Ok(resUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO userDTO)
        {
            var user = await _usersService.GetUserByUsernameAsync(userDTO.Username!);

            if (user is null)
                return Unauthorized("Invalid username or password");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userDTO.Password, user.PasswordHash);

            if (!isPasswordValid)
                return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Use true for Https/Production
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            var resUser = new ResponseUser { Id = user.Id, Username = user.UserName, Email = user.Email };

            return Ok(resUser);
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            //TODO: Aync functionalities like logging at the future. right now we dont need Async
            Response.Cookies.Delete("authToken");
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("signup")]
        public async Task<ActionResult<User>> Signup([FromBody] UserSignupDTO newUserDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _usersService.GetUserByUsernameAsync(newUserDTO.Username!);
            if (existingUser != null)
                return Conflict("Username already exists.");

            var existingEmail = await _usersService.GetUserByEmailAsync(newUserDTO.Email!);
            if (existingEmail != null)
                return Conflict("Email already registered.");

            var newUser = new User
            {
                UserName = newUserDTO.Username,
                Email = newUserDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUserDTO.Password)
            };

            await _usersService.CreateAsync(newUser);

            var token = GenerateJwtToken(newUser);

            Response.Cookies.Append("authToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Use true for Https/Production
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            var resUser = new ResponseUser { Id = newUser.Id, Username = newUser.UserName, Email = newUser.Email };

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, resUser);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, User updatedUser)
        {
            //TODO: Validation

            var user = await _usersService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            updatedUser.Id = user.Id;

            await _usersService.UpdateAsync(id, updatedUser);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _usersService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            await _usersService.RemoveAsync(id);

            return NoContent();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id!),
                ]),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
