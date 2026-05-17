FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar proyectos BuildingBlocks
COPY ["src/BuildingBlocks/AcademicSystem.Common/AcademicSystem.Common.csproj", "src/BuildingBlocks/AcademicSystem.Common/"]
COPY ["src/BuildingBlocks/AcademicSystem.EventBus/AcademicSystem.EventBus.csproj", "src/BuildingBlocks/AcademicSystem.EventBus/"]

# Copiar proyectos EnrollmentService
COPY ["src/Services/EnrollmentService/EnrollmentService.API/EnrollmentService.API.csproj", "src/Services/EnrollmentService/EnrollmentService.API/"]
COPY ["src/Services/EnrollmentService/EnrollmentService.Application/EnrollmentService.Application.csproj", "src/Services/EnrollmentService/EnrollmentService.Application/"]
COPY ["src/Services/EnrollmentService/EnrollmentService.Domain/EnrollmentService.Domain.csproj", "src/Services/EnrollmentService/EnrollmentService.Domain/"]
COPY ["src/Services/EnrollmentService/EnrollmentService.Infrastructure/EnrollmentService.Infrastructure.csproj", "src/Services/EnrollmentService/EnrollmentService.Infrastructure/"]

RUN dotnet restore "src/Services/EnrollmentService/EnrollmentService.API/EnrollmentService.API.csproj"

# Copiar todo el código fuente
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/EnrollmentService/ src/Services/EnrollmentService/

WORKDIR "/src/src/Services/EnrollmentService/EnrollmentService.API"
RUN dotnet build "EnrollmentService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnrollmentService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnrollmentService.API.dll"]
