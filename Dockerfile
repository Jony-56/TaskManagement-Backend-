# Build stage
# We use the 9.0 SDK to match your project's TargetFramework
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY ["ProjectPulse.API/ProjectPulse.API.csproj", "ProjectPulse.API/"]
COPY ["ProjectPulse.Core/ProjectPulse.Core.csproj", "ProjectPulse.Core/"]
COPY ["ProjectPulse.Infrastructure/ProjectPulse.Infrastructure.csproj", "ProjectPulse.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ProjectPulse.API/ProjectPulse.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the application
WORKDIR "/src/ProjectPulse.API"
RUN dotnet publish "ProjectPulse.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
# Use the ASP.NET 9.0 runtime for the final lean image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Standard port for .NET 8/9 containers
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProjectPulse.API.dll"]