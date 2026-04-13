using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services.Contracts;

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
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(
            IUserRepository userRepository,
            ITokenService tokenService,
            ILogger<SecurityController> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
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
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
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
                return BadRequest(new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }
    }
}