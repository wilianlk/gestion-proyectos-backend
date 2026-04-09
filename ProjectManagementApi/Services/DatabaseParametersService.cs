using DatabasesLib.Contexts;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Services;

public class DatabaseParametersService : IDatabaseParametersService
{
    private readonly DatabaseType _databaseType;
    private readonly string _connectionString;
    private readonly int _timeout;
    public DatabaseParametersService(DatabaseType databaseType, string connectionString, int timeout = 0)
    {
        _databaseType = databaseType;
        _connectionString = connectionString;
        _timeout = timeout;
    }

    public DatabaseType GetDatabaseType()
    {
        return _databaseType;
    }

    public string GetDbConnectionString()
    {
        return _connectionString;
    }

    public int GetTimeout()
    {
        return _timeout;
    }
}
