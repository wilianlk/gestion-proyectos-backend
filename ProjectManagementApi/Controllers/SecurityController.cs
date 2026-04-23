using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de seguridad y autenticación
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ISsoAuthService _ssoAuthService;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(
            IUserRepository userRepository,
            ITokenService tokenService,
            ISsoAuthService ssoAuthService,
            ILogger<SecurityController> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _ssoAuthService = ssoAuthService;
            _logger = logger;
        }

        /// <summary>
        /// Method to authenticate user and return JWT token
        /// </summary>
        /// <param name="dto">Login credentials (Username, Password)</param>
        /// <returns>JWT token if credentials are valid, 401 otherwise</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Code))
                {
                    var ssoUser = await _ssoAuthService.ResolveUserByCodeAsync(dto.Code);
                    if (ssoUser == null)
                    {
                        return Unauthorized(new { message = "Invalid or expired code" });
                    }

                    var ssoToken = _tokenService.GenerateToken(ssoUser);
                    return Ok(new TokenResponseDto
                    {
                        Token = ssoToken,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                var user = await _userRepository.GetByUsernameAsync(dto.Username);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (!Argon2.Verify(user.Password, dto.Password))
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                var token = _tokenService.GenerateToken(user);
                return Ok(new TokenResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                return ApiErrorResponse.BadRequest(this, ex, "Ocurrió un error al procesar la solicitud.");
            }
        }
    }
}
