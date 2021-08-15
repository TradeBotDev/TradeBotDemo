#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# docker build -t relay:latest -f . ..
# docker run --rm -it -p 5004:80 -p 50004:443 -e ASPNETCORE_URLS="https://+;http://+" --env-file=.env -v %USERPROFILE%\.aspnet\https:/https/ relay

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Relay/Relay.csproj"
WORKDIR "/src/Relay"
RUN dotnet build "Relay.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Relay.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Relay.dll"]