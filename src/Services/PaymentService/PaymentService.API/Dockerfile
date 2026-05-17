FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar proyectos BuildingBlocks
COPY ["src/BuildingBlocks/AcademicSystem.Common/AcademicSystem.Common.csproj", "src/BuildingBlocks/AcademicSystem.Common/"]
COPY ["src/BuildingBlocks/AcademicSystem.EventBus/AcademicSystem.EventBus.csproj", "src/BuildingBlocks/AcademicSystem.EventBus/"]

# Copiar proyectos PaymentService
COPY ["src/Services/PaymentService/PaymentService.API/PaymentService.API.csproj", "src/Services/PaymentService/PaymentService.API/"]
COPY ["src/Services/PaymentService/PaymentService.Application/PaymentService.Application.csproj", "src/Services/PaymentService/PaymentService.Application/"]
COPY ["src/Services/PaymentService/PaymentService.Domain/PaymentService.Domain.csproj", "src/Services/PaymentService/PaymentService.Domain/"]
COPY ["src/Services/PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "src/Services/PaymentService/PaymentService.Infrastructure/"]

RUN dotnet restore "src/Services/PaymentService/PaymentService.API/PaymentService.API.csproj"

# Copiar todo el código fuente
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/PaymentService/ src/Services/PaymentService/

WORKDIR "/src/src/Services/PaymentService/PaymentService.API"
RUN dotnet build "PaymentService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
