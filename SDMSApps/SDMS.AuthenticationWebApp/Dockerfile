# Dockerfile for Railway deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Accept build arguments for environment variables
# Railway automatically passes environment variables as build arguments
ARG SDMS_AuthenticationWebApp_url
ARG SDMS_AuthenticationWebApp_clientid

# Export as environment variables for Node.js build process
ENV SDMS_AuthenticationWebApp_url=${SDMS_AuthenticationWebApp_url}
ENV SDMS_AuthenticationWebApp_clientid=${SDMS_AuthenticationWebApp_clientid}

# Copy project files
COPY ["SDMS.AuthenticationWebApp.csproj", "./"]
RUN dotnet restore "SDMS.AuthenticationWebApp.csproj"

# Copy everything else
COPY . .
WORKDIR "/src"

# Build Angular app with environment variables available
WORKDIR "/src/ClientApp"
RUN echo "🔧 Building Angular ClientApp with environment variables..." && \
    echo "SDMS_AuthenticationWebApp_url=${SDMS_AuthenticationWebApp_url:-NOT_SET}" && \
    echo "SDMS_AuthenticationWebApp_clientid=${SDMS_AuthenticationWebApp_clientid:-NOT_SET}" && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install --legacy-peer-deps && \
    npm run build -- --configuration production

# Build .NET app
WORKDIR "/src"
RUN dotnet build "SDMS.AuthenticationWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SDMS.AuthenticationWebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway sets PORT environment variable automatically
# ASP.NET Core will use it via ASPNETCORE_URLS or PORT
ENV ASPNETCORE_URLS=http://+:8080

# Start the application
ENTRYPOINT ["dotnet", "SDMS.AuthenticationWebApp.dll"]

