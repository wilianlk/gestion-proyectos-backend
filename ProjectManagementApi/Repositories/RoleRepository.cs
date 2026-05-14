using System.Data;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationContext _context;
        private const string ProjectApplication = "GestionProyectos";

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
                    WHERE UPPER(TRIM(aplicacion)) = UPPER(@application)
                    ORDER BY id";

                var applicationParameter = command.CreateParameter();
                applicationParameter.ParameterName = "@application";
                applicationParameter.Value = ProjectApplication;
                command.Parameters.Add(applicationParameter);

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
                    WHERE id = @id
                      AND UPPER(TRIM(aplicacion)) = UPPER(@application)";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = id;
                command.Parameters.Add(parameter);

                var applicationParameter = command.CreateParameter();
                applicationParameter.ParameterName = "@application";
                applicationParameter.Value = ProjectApplication;
                command.Parameters.Add(applicationParameter);

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
                    WHERE UPPER(TRIM(nombre)) = UPPER(@name)
                      AND UPPER(TRIM(aplicacion)) = UPPER(@application)";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@name";
                parameter.Value = name.Trim();
                command.Parameters.Add(parameter);

                var applicationParameter = command.CreateParameter();
                applicationParameter.ParameterName = "@application";
                applicationParameter.Value = ProjectApplication;
                command.Parameters.Add(applicationParameter);

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

        public async Task<Role> CreateAsync(Role role)
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
                    INSERT INTO requisiciones_roles (nombre, descripcion, aplicacion)
                    VALUES (@name, @description, @application)";

                var nameParameter = command.CreateParameter();
                nameParameter.ParameterName = "@name";
                nameParameter.Value = role.Name.Trim();
                command.Parameters.Add(nameParameter);

                var descriptionParameter = command.CreateParameter();
                descriptionParameter.ParameterName = "@description";
                descriptionParameter.Value = string.IsNullOrWhiteSpace(role.Description)
                    ? DBNull.Value
                    : role.Description.Trim();
                command.Parameters.Add(descriptionParameter);

                var applicationParameter = command.CreateParameter();
                applicationParameter.ParameterName = "@application";
                applicationParameter.Value = ProjectApplication;
                command.Parameters.Add(applicationParameter);

                await command.ExecuteNonQueryAsync();
                var createdRole = await GetByNameAsync(role.Name.Trim());

                return createdRole ?? new Role
                {
                    Name = role.Name.Trim(),
                    Description = role.Description?.Trim(),
                    Application = ProjectApplication
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
