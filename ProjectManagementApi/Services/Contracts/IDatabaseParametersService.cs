using DatabasesLib.Contexts;

namespace ProjectManagementApi.Services.Contracts;

public interface IDatabaseParametersService
{
    string GetDbConnectionString();
    DatabaseType GetDatabaseType();
    int GetTimeout();
}
