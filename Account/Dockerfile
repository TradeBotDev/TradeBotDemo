# Команды для запуска:
# Windows: docker build -t account:latest -f . ..
# Windows: docker run --rm -it -p 5000:80 -p 12340:12340 -e ASPNETCORE_URLS="http://+;" -e POSTGRES_CONNECTION_STRING=Host=host.docker.internal;Port=5432;Database=accountdb;Username=postgres;Password=password account
# Linux: sudo docker build -t account:latest -f Dockerfile ..
# Linux: sudo docker run --rm -it -p 5000:80 -p 12340:12340 -e ASPNETCORE_URLS="http://+;" account

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 12340

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Account/Account.csproj"
WORKDIR "/src/Account"
RUN dotnet build "Account.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Account.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Account.dll"]