# ==========================================
# Stage 1: Runtime Base (Alpine Linux)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# ==========================================
# Stage 2: SDK Build Environment
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Leverage Docker layer caching by copying .csproj files first
COPY ["Employee.API/Employee.API.csproj", "Employee.API/"]
COPY ["Employee.Application/Employee.Application.csproj", "Employee.Application/"]
COPY ["Employee.Domain/Employee.Domain.csproj", "Employee.Domain/"]
COPY ["Employee.Infrastructure/Employee.Infrastructure.csproj", "Employee.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Employee.API/Employee.API.csproj"

# Copy the remaining project source files
COPY . .

# Build the API project
WORKDIR "/src/Employee.API"
RUN dotnet build "Employee.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# Stage 3: Publish
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Employee.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# Stage 4: Final Production Image
# ==========================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user (built into .NET 8 Alpine) for enhanced security
USER app

ENTRYPOINT ["dotnet", "Employee.API.dll"]
