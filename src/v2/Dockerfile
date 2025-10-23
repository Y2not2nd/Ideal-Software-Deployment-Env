# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ClickCounterApp.csproj .
RUN dotnet restore ClickCounterApp.csproj

# Copy the rest of the source code
COPY . .

# Publish the app
RUN dotnet publish ClickCounterApp.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .

# Start the app
ENTRYPOINT ["dotnet", "ClickCounterApp.dll"]
