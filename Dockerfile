FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

# copy everything else and build app
WORKDIR /
COPY src/. .

# build BackendApi
WORKDIR /BackendApi
RUN dotnet publish -c Release -o bin

# build Frontend
WORKDIR /Frontend
RUN dotnet publish -c Release -o bin

# build JobLogger
WORKDIR /JobLogger
RUN dotnet publish -c Release -o bin

# build TextRankCalc
WORKDIR /TextRankCalc
RUN dotnet publish -c Release -o bin

# BackendApi runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS backend-api
WORKDIR /BackendApi
COPY --from=build /BackendApi/bin ./

ENTRYPOINT ["dotnet", "BackendApi.dll"]

# Frontend runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS frontend
WORKDIR /Frontend
COPY --from=build /Frontend/bin ./

ENTRYPOINT ["dotnet", "Frontend.dll"]

# JobLogger runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS job-logger
WORKDIR /JobLogger
COPY --from=build /JobLogger/bin ./

ENTRYPOINT ["dotnet", "JobLogger.dll"]

# JobLogger runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS text-rank-calc
WORKDIR /TextRankCalc
COPY --from=build /TextRankCalc/bin ./

ENTRYPOINT ["dotnet", "TextRankCalc.dll"]