# ProjectManagementApi

API Web en ASP.NET Core para la gestión de menús.

## Cómo ejecutar

1. Abrir la carpeta `ProjectManagementApi` en Visual Studio o Visual Studio Code.
2. Restaurar paquetes NuGet.
3. Ejecutar el proyecto.
4. Acceder a la API en `https://localhost:{puerto}`.

## Estructura básica

- `ProjectManagementApi/ProjectManagementApi.csproj` - proyecto principal.
- `ProjectManagementApi/Controllers` - controladores Web API.
- `ProjectManagementApi/Models` - entidades de dominio.
- `ProjectManagementApi/DTO` - objetos de transferencia de datos.
- `ProjectManagementApi/Repositories` - acceso a datos.
- `ProjectManagementApi/Services` - lógica de negocio.

## Requisitos

- .NET 8 SDK o superior.

## Notas

- Ignora los archivos generados en `bin/`, `obj/` y carpetas temporales del IDE.
- Observabilidad mínima disponible en `GET /api/Observability/ErrorDashboard` (requiere autenticación).
- Las respuestas de error incluyen `traceId` y `category` para facilitar diagnóstico.
