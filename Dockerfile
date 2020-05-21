FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

# Restore BackendApi
WORKDIR /ds-3/BackendApi
COPY src/BackendApi/*.csproj .
RUN dotnet restore

# Restore Frontend
WORKDIR /ds-3/Frontend
COPY src/Frontend/*.csproj .
RUN dotnet restore

# Restore JobLogger
WORKDIR /ds-3/JobLogger
COPY src/JobLogger/*.csproj .
RUN dotnet restore

# copy everything else and build app
WORKDIR /ds-3
COPY src/. .

# build BackendApi
WORKDIR /ds-3/BackendApi
RUN dotnet publish -c Release -o bin

# build Frontend
WORKDIR /ds-3/Frontend
RUN dotnet publish -c Release -o bin

# build JobLogger
WORKDIR /ds-3/JobLogger
RUN dotnet publish -c Release -o bin

# BackendApi runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS backend-api
WORKDIR /BackendApi
COPY --from=build /ds-3/BackendApi/bin ./

ENTRYPOINT ["dotnet", "BackendApi.dll"]

# Frontend runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS frontend
WORKDIR /Frontend
COPY --from=build /ds-3/Frontend/bin ./

ENTRYPOINT ["dotnet", "Frontend.dll"]

# JobLogger runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS job-logger
WORKDIR /JobLogger
COPY --from=build /ds-3/JobLogger/bin ./

ENTRYPOINT ["dotnet", "JobLogger.dll"]