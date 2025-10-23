# e.g. src/v1/Dockerfile and src/v2/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy project and restore
COPY *.csproj ./
RUN dotnet restore

# Copy remaining source and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Run stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .

# Use HTTPS redirection (App Service will handle TLS termination)
ENTRYPOINT ["dotnet", "ClickCounterApp.dll"]
