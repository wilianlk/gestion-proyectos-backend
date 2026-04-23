using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Services
{
    public class SsoAuthService : ISsoAuthService
    {
        private readonly ApplicationContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public SsoAuthService(
            ApplicationContext context,
            IUserRepository userRepository,
            IRoleRepository roleRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<User?> ResolveUserByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                var identification = await GetIdentificationByCodeAsync(connection, code.Trim());
                if (string.IsNullOrWhiteSpace(identification))
                {
                    return null;
                }

                var authUser = await GetAuthUserByIdentificationAsync(connection, identification);
                if (authUser == null)
                {
                    return null;
                }

                var localUser = await _userRepository.GetByIdentificationAsync(identification);
                if (localUser != null)
                {
                    localUser.Name = string.IsNullOrWhiteSpace(localUser.Name) ? authUser.Name : localUser.Name;
                    localUser.LastName = string.IsNullOrWhiteSpace(localUser.LastName) ? authUser.LastName : localUser.LastName;
                    localUser.Email = string.IsNullOrWhiteSpace(localUser.Email) ? authUser.Email : localUser.Email;
                    localUser.Identification = identification;
                    return localUser;
                }

                var role = await _roleRepository.GetByNameAsync("Viewer")
                    ?? new Role
                    {
                        Id = 0,
                        Name = "Viewer",
                        Description = "Viewer"
                    };

                authUser.Id = 0;
                authUser.RoleId = role.Id;
                authUser.Role = role;
                authUser.Password = string.Empty;
                authUser.IsActive = true;

                return authUser;
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static async Task<string?> GetIdentificationByCodeAsync(DbConnection connection, string code)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT FIRST 1 TRIM(usuario_cedula)
                FROM sirii_sso_codes
                WHERE code = @code
                  AND consumed_at_utc IS NULL
                  AND expires_at_utc > CURRENT";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@code";
            parameter.Value = code;
            command.Parameters.Add(parameter);

            return (await command.ExecuteScalarAsync())?.ToString()?.Trim();
        }

        private static async Task<User?> GetAuthUserByIdentificationAsync(DbConnection connection, string identification)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT FIRST 1
                    TRIM(solicitante_identificacion) AS identificacion,
                    TRIM(solicitante_nombre) AS nombre,
                    TRIM(solicitante_email) AS correo
                FROM solicitudes_aprobaciones
                WHERE TRIM(solicitante_identificacion) = @identification
                ORDER BY id DESC";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@identification";
            parameter.Value = identification;
            command.Parameters.Add(parameter);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var fullName = reader["nombre"]?.ToString()?.Trim() ?? identification;
            var splitName = SplitName(fullName);

            return new User
            {
                Username = identification,
                Identification = reader["identificacion"]?.ToString()?.Trim() ?? identification,
                Email = reader["correo"]?.ToString()?.Trim() ?? string.Empty,
                Name = splitName.Name,
                LastName = splitName.LastName
            };
        }

        private static (string Name, string LastName) SplitName(string fullName)
        {
            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length <= 1)
            {
                return (fullName, string.Empty);
            }

            return (parts[0], string.Join(' ', parts.Skip(1)));
        }
    }
}
