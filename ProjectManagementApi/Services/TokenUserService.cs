using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Services
{
    /// <summary>
    /// Servicio para extraer información del usuario desde el token JWT
    /// </summary>
    public class TokenUserService : ITokenUserService
    {
        private readonly IUserRepository _userRepository;

        public TokenUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        /// <summary>
        /// Description: Extract user information from JWT claims
        /// Input Parameters: 
        ///     * claimsPrincipal (ClaimsPrincipal): JWT claims from the request
        /// Output Parameters: User object with basic information (Id, Username, Name, LastName, RoleId, Role)
        /// </summary>
        public async Task<User?> GetCurrentUser(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                return null;

            // Find the serialized user data claim
            var userClaim = claimsPrincipal.FindFirst("user");
            if (userClaim == null)
                return null;

            try
            {
                // Deserialize the user data from the claim
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userClaim.Value);
                if (userData == null)
                    return null;

                // Extract user from DB
                var username = userData["Username"].GetString();
                if (string.IsNullOrEmpty(username))
                    return null;

                // Retornar usuario completo desde la base de datos (incluye Role)
                return await _userRepository.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deserializing user from token: {ex.Message}");
                return null;
            }
        }
    }
}