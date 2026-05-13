using System.Data;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationContext _context;

        public RoleRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var roles = new List<Role>();
            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT id,
                           TRIM(nombre) AS nombre,
                           TRIM(descripcion) AS descripcion,
                           TRIM(aplicacion) AS aplicacion
                    FROM requisiciones_roles
                    ORDER BY id";

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    roles.Add(new Role
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["nombre"]?.ToString()?.Trim() ?? string.Empty,
                        Description = reader["descripcion"]?.ToString()?.Trim(),
                        Application = reader["aplicacion"]?.ToString()?.Trim()
                    });
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }

            return roles;
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT FIRST 1 id,
                                   TRIM(nombre) AS nombre,
                                   TRIM(descripcion) AS descripcion,
                                   TRIM(aplicacion) AS aplicacion
                    FROM requisiciones_roles
                    WHERE id = @id";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                return new Role
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Name = reader["nombre"]?.ToString()?.Trim() ?? string.Empty,
                    Description = reader["descripcion"]?.ToString()?.Trim(),
                    Application = reader["aplicacion"]?.ToString()?.Trim()
                };
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT FIRST 1 id,
                                   TRIM(nombre) AS nombre,
                                   TRIM(descripcion) AS descripcion,
                                   TRIM(aplicacion) AS aplicacion
                    FROM requisiciones_roles
                    WHERE UPPER(TRIM(nombre)) = UPPER(@name)";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@name";
                parameter.Value = name.Trim();
                command.Parameters.Add(parameter);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                return new Role
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Name = reader["nombre"]?.ToString()?.Trim() ?? string.Empty,
                    Description = reader["descripcion"]?.ToString()?.Trim(),
                    Application = reader["aplicacion"]?.ToString()?.Trim()
                };
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
