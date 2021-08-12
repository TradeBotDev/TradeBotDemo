#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# docker build -t former:latest -f . ..
# docker run --rm -it -p 5003:80 -p 50003:443 -e ASPNETCORE_URLS="https://+;http://+" --env-file=.env -v %USERPROFILE%\.aspnet\https:/https/ former

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Former/Former.csproj"
WORKDIR "/src/Former"
RUN dotnet build "Former.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Former.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Former.dll"]