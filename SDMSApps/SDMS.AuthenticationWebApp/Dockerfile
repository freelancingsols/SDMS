# Dockerfile for Railway deployment (optional - Railway can auto-detect .NET)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SDMS.AuthenticationWebApp.csproj", "./"]
RUN dotnet restore "SDMS.AuthenticationWebApp.csproj"

# Copy everything else
COPY . .
WORKDIR "/src"

# Build Angular app
WORKDIR "/src/ClientApp"
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    npm ci && \
    npm run build

# Build .NET app
WORKDIR "/src"
RUN dotnet build "SDMS.AuthenticationWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SDMS.AuthenticationWebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SDMS.AuthenticationWebApp.dll"]
