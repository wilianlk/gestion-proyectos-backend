using System.Security.Claims;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Services.Contracts
{
    /// <summary>
    /// Interfaz para el servicio de extracción de usuario desde el token
    /// </summary>
    public interface ITokenUserService
    {
        /// <summary>
        /// Extract user information from JWT claims
        /// </summary>
        /// <param name="claimsPrincipal">JWT claims from the request</param>
        /// <returns>User object with basic information</returns>
        Task<User?> GetCurrentUser(ClaimsPrincipal claimsPrincipal);
    }
}