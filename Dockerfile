# Set the base image for the SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY sources/*.sln .
COPY sources/CinemaRolfoBot/*.csproj ./CinemaRolfoBot/
RUN dotnet restore

# Copy the remaining source files and build the project
COPY sources/. ./
RUN dotnet publish -c Release -o out

# Set the base image for the runtime
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/out .

# Set the entrypoint for the application
ENTRYPOINT ["dotnet", "CinemaRolfoBot.dll"]
