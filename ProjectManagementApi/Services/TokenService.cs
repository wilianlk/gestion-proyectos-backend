using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementApi.Models;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils.Helpers;

namespace ProjectManagementApi.Services
{
    /// <summary>
    /// Servicio para la generación de tokens JWT
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        public TokenService(
            IConfiguration configuration,
            IOptions<AppSettings> appSettings
            )
        {
            _configuration = configuration;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Description: Generate a JWT token for a user
        /// Input Parameters: 
        ///     * user (User): User object containing user information
        /// Output Parameters: JWT token string
        /// </summary>
        public string GenerateToken(User user)
        {
            ClaimsIdentity claims = new ClaimsIdentity();

            var data = new Dictionary<string, dynamic>
            {
                {"UserId", user.Id},
                {"Username", user.Username},
                {"Email", user.Email},
                {"IsActive", user.IsActive},
                {"LastName", user.LastName},
                {"Name", user.Name},
                {"Role", user.Role}
            };
            var serializer = JsonSerializer.Serialize(data);
            const string key = "user";

            claims.AddClaim(new Claim(key, serializer));
            claims.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            claims.AddClaim(new Claim(ClaimTypes.Role, user.Role.Name));
            claims.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            SymmetricSecurityKey secretKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Jwt.Key));
            SigningCredentials signingCredentials =
                new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Jwt.Issuer,
                Audience = _appSettings.Jwt.Audience,
                Subject = claims,
                Expires = DateTime.Now.AddHours(_appSettings.Jwt.Expired),
                SigningCredentials = signingCredentials
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }
}