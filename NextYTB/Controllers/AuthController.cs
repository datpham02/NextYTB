using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using NextYTB.IRequest;
using NextYTB.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace NextYTB.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly NextTubContext _dbContext;
        private readonly IConfiguration _config;

        public AuthController(NextTubContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [HttpGet("get_accessToken")]
        public async Task<ActionResult> GetAccessToken(int id)
        {
            var account = _dbContext.Accounts.FirstOrDefault(a => a.Id == id);
            var checkRefreshToken = account?.RefreshToken;

            var verifyToken = await VerifyToken(checkRefreshToken);
            if (verifyToken)
            {

                var accessToken = GenerateToken(account, 1 * 60 * 60 * 24);

                return StatusCode(StatusCodes.Status200OK, new { success = true, accessToken });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Refresh is invalid " });
        }

        [HttpPost("verify")]
        public async Task<ActionResult> Verify([FromBody] IVerifyToken data)
        {
            if (data.token != null)
            {
                var verifyToken = await VerifyToken(data.token);
                if (verifyToken)
                {
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }
                else return StatusCode(StatusCodes.Status404NotFound, new { success = false });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Invalid token" });
        }
        private async Task<bool> VerifyToken(string token)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
            SecurityToken securityToken;
            var principal = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);

            if (principal.Claims != null)
            {
                return true;
            }
            return false;
        }





        private string GenerateToken(Account user, int expiresSecond)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Id",user.Id.ToString()),
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddMilliseconds(expiresSecond),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
