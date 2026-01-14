## ContosoPizza (Blazor WebAssembly)

This repo runs the UI as a Blazor WebAssembly app and exposes a separate ASP.NET Core backend API.

- Client: `src/ContosoPizza.Client` (Blazor WebAssembly)
- Server: `src/ContosoPizza.Server` (ASP.NET Core + EF Core + PostgreSQL + API)
- Shared: `src/ContosoPizza.Shared` (shared models)

### Run (split frontend/backend)

1) Start PostgreSQL (or update `src/ContosoPizza.Server/appsettings*.json`)

2) Run the backend API:

`dotnet run --project src/ContosoPizza.Server/ContosoPizza.Server.csproj`

3) Run the frontend:

`dotnet run --project src/ContosoPizza.Client/ContosoPizza.Client.csproj`

The frontend calls the backend using `ApiBaseUrl` (or `ApiBaseUrlHttp`/`ApiBaseUrlHttps` in Development) from `src/ContosoPizza.Client/wwwroot/appsettings*.json`.

To allow the browser to call the API cross-origin in development, update allowed origins in `src/ContosoPizza.Server/appsettings.Development.json` under `Cors:AllowedOrigins`.

### API

- `GET /api/pizza`
- `POST /api/pizza`
- `PUT /api/pizza/{id}`
- `DELETE /api/pizza/{id}`
