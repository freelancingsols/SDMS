# Dockerfile for Railway deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SDMS.GatewayApi.csproj", "./"]
RUN dotnet restore "SDMS.GatewayApi.csproj"

# Copy everything else
COPY . .
WORKDIR "/src"

# Build .NET app
RUN dotnet build "SDMS.GatewayApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SDMS.GatewayApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway sets PORT environment variable automatically
# ASP.NET Core will use it via ASPNETCORE_URLS or PORT
ENV ASPNETCORE_URLS=http://+:8080

# Start the application
ENTRYPOINT ["dotnet", "SDMS.GatewayApi.dll"]

