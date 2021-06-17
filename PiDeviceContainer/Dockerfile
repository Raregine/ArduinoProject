FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# copy and build app
COPY . .
RUN dotnet publish -c release -o out

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim-arm32v7
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "PiDeviceContainer.dll"]
