FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar proyectos BuildingBlocks
COPY ["src/BuildingBlocks/AcademicSystem.Common/AcademicSystem.Common.csproj", "src/BuildingBlocks/AcademicSystem.Common/"]

COPY ["src/ApiGateway/AcademicSystem.ApiGateway/AcademicSystem.ApiGateway.csproj", "src/ApiGateway/AcademicSystem.ApiGateway/"]
RUN dotnet restore "src/ApiGateway/AcademicSystem.ApiGateway/AcademicSystem.ApiGateway.csproj"

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/ApiGateway/AcademicSystem.ApiGateway/ src/ApiGateway/AcademicSystem.ApiGateway/

WORKDIR "/src/src/ApiGateway/AcademicSystem.ApiGateway"
RUN dotnet build "AcademicSystem.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcademicSystem.ApiGateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Instalar curl para health checks
RUN apt-get update && apt-get install -y curl

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AcademicSystem.ApiGateway.dll"]