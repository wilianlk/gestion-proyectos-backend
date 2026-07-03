namespace ProjectManagementApi.Services.Contracts
{
    /// <summary>
    /// Interfaz para el servicio de generación de tokens JWT
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate a JWT token for a user
        /// </summary>
        /// <param name="user">User object</param>
        /// <returns>Token response with token string and expiration date</returns>
        DTO.TokenResponseDto GenerateToken(Models.User user);
    }
}