FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["StudentService.API/StudentService.API.csproj", "StudentService.API/"]
COPY ["StudentService.Application/StudentService.Application.csproj", "StudentService.Application/"]
COPY ["StudentService.Domain/StudentService.Domain.csproj", "StudentService.Domain/"]
COPY ["StudentService.Infrastructure/StudentService.Infrastructure.csproj", "StudentService.Infrastructure/"]
RUN dotnet restore "StudentService.API/StudentService.API.csproj"

COPY . .
WORKDIR "/src/StudentService.API"
RUN dotnet build "StudentService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentService.API.dll"]