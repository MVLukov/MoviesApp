FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /app
COPY ./MoviesApp.Data/ ./MoviesApp.Data/
COPY ./MoviesApp.DataSeeder/ ./MoviesApp.DataSeeder/

WORKDIR /app/MoviesApp.Data
RUN dotnet restore

WORKDIR /app/MoviesApp.DataSeeder
RUN dotnet restore

WORKDIR /app/MoviesApp.Web
COPY ./MoviesApp.Web/ ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY .env ./
COPY ./MoviesApp.Web/data ./data
COPY --from=build-env /app/MoviesApp.Web/out .
ENTRYPOINT ["dotnet", "MoviesApp.Web.dll"]
RUN apt update && apt upgrade -y && apt install curl -y
HEALTHCHECK CMD curl -k https://localhost:443/health || exit
