using System.Data;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationContext _context;

        public UserRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            const string sql = @"
                SELECT FIRST 1
                    sa.id,
                    TRIM(sa.solicitante_identificacion) AS identificacion,
                    TRIM(sa.solicitante_nombre) AS nombre,
                    TRIM(sa.solicitante_email) AS correo,
                    (SELECT MIN(r.id)
                     FROM requisiciones_solicitantes_roles sr
                     JOIN requisiciones_roles r ON r.id = sr.rol_id
                     WHERE TRIM(sr.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)) AS role_id,
                    (SELECT TRIM(r.nombre)
                     FROM requisiciones_roles r
                     WHERE r.id = (
                         SELECT MIN(r2.id)
                         FROM requisiciones_solicitantes_roles sr2
                         JOIN requisiciones_roles r2 ON r2.id = sr2.rol_id
                         WHERE TRIM(sr2.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)
                     )) AS role_name
                FROM solicitudes_aprobaciones sa
                WHERE sa.id = (
                        SELECT MAX(sa2.id)
                        FROM solicitudes_aprobaciones sa2
                        WHERE TRIM(sa2.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)
                    )
                  AND (
                        UPPER(TRIM(sa.solicitante_identificacion)) = UPPER(@username)
                        OR UPPER(TRIM(sa.solicitante_email)) = UPPER(@username)
                    )";

            return await QuerySingleUserAsync(sql, ("@username", username.Trim()));
        }

        public async Task<User?> GetByIdentificationAsync(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
            {
                return null;
            }

            const string sql = @"
                SELECT FIRST 1
                    sa.id,
                    TRIM(sa.solicitante_identificacion) AS identificacion,
                    TRIM(sa.solicitante_nombre) AS nombre,
                    TRIM(sa.solicitante_email) AS correo,
                    (SELECT MIN(r.id)
                     FROM requisiciones_solicitantes_roles sr
                     JOIN requisiciones_roles r ON r.id = sr.rol_id
                     WHERE TRIM(sr.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)) AS role_id,
                    (SELECT TRIM(r.nombre)
                     FROM requisiciones_roles r
                     WHERE r.id = (
                         SELECT MIN(r2.id)
                         FROM requisiciones_solicitantes_roles sr2
                         JOIN requisiciones_roles r2 ON r2.id = sr2.rol_id
                         WHERE TRIM(sr2.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)
                     )) AS role_name
                FROM solicitudes_aprobaciones sa
                WHERE TRIM(sa.solicitante_identificacion) = @identification
                ORDER BY sa.id DESC";

            return await QuerySingleUserAsync(sql, ("@identification", identification.Trim()));
        }

        public async Task<IEnumerable<User>> GetAllWithRoleAsync()
        {
            const string sql = @"
                SELECT
                    sa.id,
                    TRIM(sa.solicitante_identificacion) AS identificacion,
                    TRIM(sa.solicitante_nombre) AS nombre,
                    TRIM(sa.solicitante_email) AS correo,
                    (SELECT MIN(r.id)
                     FROM requisiciones_solicitantes_roles sr
                     JOIN requisiciones_roles r ON r.id = sr.rol_id
                     WHERE TRIM(sr.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)) AS role_id,
                    (SELECT TRIM(r.nombre)
                     FROM requisiciones_roles r
                     WHERE r.id = (
                         SELECT MIN(r2.id)
                         FROM requisiciones_solicitantes_roles sr2
                         JOIN requisiciones_roles r2 ON r2.id = sr2.rol_id
                         WHERE TRIM(sr2.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)
                     )) AS role_name
                FROM solicitudes_aprobaciones sa
                WHERE sa.id = (
                    SELECT MAX(sa2.id)
                    FROM solicitudes_aprobaciones sa2
                    WHERE TRIM(sa2.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)
                )
                ORDER BY sa.id DESC";

            return await QueryUsersAsync(sql);
        }

        public async Task<User?> GetByIdWithRoleAsync(int id)
        {
            const string sql = @"
                SELECT FIRST 1
                    sa.id,
                    TRIM(sa.solicitante_identificacion) AS identificacion,
                    TRIM(sa.solicitante_nombre) AS nombre,
                    TRIM(sa.solicitante_email) AS correo,
                    (SELECT MIN(r.id)
                     FROM requisiciones_solicitantes_roles sr
                     JOIN requisiciones_roles r ON r.id = sr.rol_id
                     WHERE TRIM(sr.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)) AS role_id,
                    (SELECT TRIM(r.nombre)
                     FROM requisiciones_roles r
                     WHERE r.id = (
                         SELECT MIN(r2.id)
                         FROM requisiciones_solicitantes_roles sr2
                         JOIN requisiciones_roles r2 ON r2.id = sr2.rol_id
                         WHERE TRIM(sr2.solicitante_identificacion) = TRIM(sa.solicitante_identificacion)
                     )) AS role_name
                FROM solicitudes_aprobaciones sa
                WHERE sa.id = @id";

            return await QuerySingleUserAsync(sql, ("@id", id));
        }

        private async Task<User?> QuerySingleUserAsync(string sql, params (string Name, object Value)[] parameters)
        {
            var users = await QueryUsersAsync(sql, parameters);
            return users.FirstOrDefault();
        }

        private async Task<List<User>> QueryUsersAsync(string sql, params (string Name, object Value)[] parameters)
        {
            var users = new List<User>();
            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;

                foreach (var (name, value) in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = name;
                    parameter.Value = value;
                    command.Parameters.Add(parameter);
                }

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(MapUser(reader));
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }

            return users;
        }

        private static User MapUser(IDataRecord reader)
        {
            var fullName = reader["nombre"]?.ToString()?.Trim() ?? string.Empty;
            var roleName = reader["role_name"]?.ToString()?.Trim() ?? "Usuario";
            var roleId = reader["role_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["role_id"]);
            var splitName = SplitName(fullName);

            return new User
            {
                Id = Convert.ToInt32(reader["id"]),
                Username = reader["identificacion"]?.ToString()?.Trim() ?? string.Empty,
                Identification = reader["identificacion"]?.ToString()?.Trim() ?? string.Empty,
                Email = reader["correo"]?.ToString()?.Trim() ?? string.Empty,
                Name = splitName.Name,
                LastName = splitName.LastName,
                Password = string.Empty,
                IsActive = true,
                RoleId = roleId,
                Role = new Role
                {
                    Id = roleId,
                    Name = roleName,
                    Description = roleName
                }
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
