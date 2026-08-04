# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solución y archivos de proyecto
COPY ["backend/MipymeAsistencia.sln", "backend/"]
COPY ["backend/src/MipymeAsistencia.Domain/MipymeAsistencia.Domain.csproj", "backend/src/MipymeAsistencia.Domain/"]
COPY ["backend/src/MipymeAsistencia.Application/MipymeAsistencia.Application.csproj", "backend/src/MipymeAsistencia.Application/"]
COPY ["backend/src/MipymeAsistencia.Infrastructure/MipymeAsistencia.Infrastructure.csproj", "backend/src/MipymeAsistencia.Infrastructure/"]
COPY ["backend/src/MipymeAsistencia.WebApi/MipymeAsistencia.WebApi.csproj", "backend/src/MipymeAsistencia.WebApi/"]

# Restaurar dependencias
RUN dotnet restore "backend/MipymeAsistencia.sln"

# Copiar todo el proyecto y publicar
COPY backend/ ./backend/
WORKDIR /src/backend
RUN dotnet publish "src/MipymeAsistencia.WebApi/MipymeAsistencia.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MipymeAsistencia.WebApi.dll"]
