FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar proyectos BuildingBlocks
COPY ["src/BuildingBlocks/AcademicSystem.Common/AcademicSystem.Common.csproj", "src/BuildingBlocks/AcademicSystem.Common/"]
COPY ["src/BuildingBlocks/AcademicSystem.EventBus/AcademicSystem.EventBus.csproj", "src/BuildingBlocks/AcademicSystem.EventBus/"]

# Copiar proyectos StudentService
COPY ["src/Services/StudentService/StudentService.API/StudentService.API.csproj", "src/Services/StudentService/StudentService.API/"]
COPY ["src/Services/StudentService/StudentService.Application/StudentService.Application.csproj", "src/Services/StudentService/StudentService.Application/"]
COPY ["src/Services/StudentService/StudentService.Domain/StudentService.Domain.csproj", "src/Services/StudentService/StudentService.Domain/"]
COPY ["src/Services/StudentService/StudentService.Infrastructure/StudentService.Infrastructure.csproj", "src/Services/StudentService/StudentService.Infrastructure/"]

RUN dotnet restore "src/Services/StudentService/StudentService.API/StudentService.API.csproj"

# Copiar todo el código fuente
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/StudentService/ src/Services/StudentService/

WORKDIR "/src/src/Services/StudentService/StudentService.API"
RUN dotnet build "StudentService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentService.API.dll"]