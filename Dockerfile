# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ProjectPulse.API/ProjectPulse.API.csproj", "ProjectPulse.API/"]
COPY ["ProjectPulse.Core/ProjectPulse.Core.csproj", "ProjectPulse.Core/"]
COPY ["ProjectPulse.Infrastructure/ProjectPulse.Infrastructure.csproj", "ProjectPulse.Infrastructure/"]

RUN dotnet restore "ProjectPulse.API/ProjectPulse.API.csproj"

COPY . .

WORKDIR "/src/ProjectPulse.API"
RUN dotnet publish "ProjectPulse.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProjectPulse.API.dll"]