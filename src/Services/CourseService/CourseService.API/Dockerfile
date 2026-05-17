FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar proyectos BuildingBlocks
COPY ["src/BuildingBlocks/AcademicSystem.Common/AcademicSystem.Common.csproj", "src/BuildingBlocks/AcademicSystem.Common/"]
COPY ["src/BuildingBlocks/AcademicSystem.EventBus/AcademicSystem.EventBus.csproj", "src/BuildingBlocks/AcademicSystem.EventBus/"]

# Copiar proyectos CourseService
COPY ["src/Services/CourseService/CourseService.API/CourseService.API.csproj", "src/Services/CourseService/CourseService.API/"]
COPY ["src/Services/CourseService/CourseService.Application/CourseService.Application.csproj", "src/Services/CourseService/CourseService.Application/"]
COPY ["src/Services/CourseService/CourseService.Domain/CourseService.Domain.csproj", "src/Services/CourseService/CourseService.Domain/"]
COPY ["src/Services/CourseService/CourseService.Infrastructure/CourseService.Infrastructure.csproj", "src/Services/CourseService/CourseService.Infrastructure/"]

RUN dotnet restore "src/Services/CourseService/CourseService.API/CourseService.API.csproj"

# Copiar todo el código fuente
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/CourseService/ src/Services/CourseService/

WORKDIR "/src/src/Services/CourseService/CourseService.API"
RUN dotnet build "CourseService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CourseService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CourseService.API.dll"]
