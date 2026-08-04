# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solución y archivos de proyecto (rutas relativas a la carpeta backend)
COPY ["MipymeAsistencia.sln", "./"]
COPY ["src/MipymeAsistencia.Domain/MipymeAsistencia.Domain.csproj", "src/MipymeAsistencia.Domain/"]
COPY ["src/MipymeAsistencia.Application/MipymeAsistencia.Application.csproj", "src/MipymeAsistencia.Application/"]
COPY ["src/MipymeAsistencia.Infrastructure/MipymeAsistencia.Infrastructure.csproj", "src/MipymeAsistencia.Infrastructure/"]
COPY ["src/MipymeAsistencia.WebApi/MipymeAsistencia.WebApi.csproj", "src/MipymeAsistencia.WebApi/"]

# Restaurar dependencias
RUN dotnet restore "MipymeAsistencia.sln"

# Copiar todo el contenido del backend y compilar
COPY . .
WORKDIR /src/src/MipymeAsistencia.WebApi
RUN dotnet publish "MipymeAsistencia.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MipymeAsistencia.WebApi.dll"]