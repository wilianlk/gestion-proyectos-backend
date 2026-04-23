using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using ProjectManagementApi.Models;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Services
{
    /// <summary>
    /// Servicio para extraer información del usuario desde el token JWT
    /// </summary>
    public class TokenUserService : ITokenUserService
    {
        /// <summary>
        /// Description: Extract user information from JWT claims
        /// Input Parameters: 
        ///     * claimsPrincipal (ClaimsPrincipal): JWT claims from the request
        /// Output Parameters: User object with basic information (Id, Username, Name, LastName, RoleId, Role)
        /// </summary>
        public Task<User?> GetCurrentUser(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                return Task.FromResult<User?>(null);

            // Find the serialized user data claim
            var userClaim = claimsPrincipal.FindFirst("user");
            if (userClaim == null)
                return Task.FromResult<User?>(null);

            try
            {
                // Deserialize the user data from the claim
                var userData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userClaim.Value);
                if (userData == null)
                    return Task.FromResult<User?>(null);

                var roleElement = userData["Role"];
                var role = new Role
                {
                    Id = roleElement.TryGetProperty("Id", out var roleId) ? roleId.GetInt32() : 0,
                    Name = roleElement.TryGetProperty("Name", out var roleName) ? roleName.GetString() ?? string.Empty : string.Empty,
                    Description = roleElement.TryGetProperty("Description", out var roleDescription) ? roleDescription.GetString() : null
                };

                var user = new User
                {
                    Id = userData.TryGetValue("UserId", out var userId) ? userId.GetInt32() : 0,
                    Username = userData.TryGetValue("Username", out var username) ? username.GetString() ?? string.Empty : string.Empty,
                    Email = userData.TryGetValue("Email", out var email) ? email.GetString() ?? string.Empty : string.Empty,
                    Identification = userData.TryGetValue("Identification", out var identification) ? identification.GetString() ?? string.Empty : string.Empty,
                    Name = userData.TryGetValue("Name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                    LastName = userData.TryGetValue("LastName", out var lastName) ? lastName.GetString() ?? string.Empty : string.Empty,
                    IsActive = userData.TryGetValue("IsActive", out var isActive) && isActive.ValueKind is JsonValueKind.True or JsonValueKind.False
                        ? isActive.GetBoolean()
                        : true,
                    Role = role,
                    RoleId = role.Id
                };

                return Task.FromResult<User?>(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deserializing user from token: {ex.Message}");
                return Task.FromResult<User?>(null);
            }
        }
    }
}
