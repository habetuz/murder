# Stage 1: Build frontend
FROM oven/bun:1 AS frontend-build
WORKDIR /app/frontend
COPY frontend/package.json frontend/bun.lock* ./
RUN bun install --frozen-lockfile
COPY frontend/ .
ENV VITE_API_BASE_URL=""
RUN bun run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS backend-build
WORKDIR /app
COPY Murder.sln ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore
RUN dotnet publish src/Murder.Plugins/WebAPI/Murder.Plugins.WebAPI.csproj -c Release -o /publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=backend-build /publish .
COPY --from=frontend-build /app/frontend/dist wwwroot/

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Murder.Plugins.WebAPI.dll"]
