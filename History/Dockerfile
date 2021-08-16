#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# docker build -t history:latest -f . ..
# docker run --rm -it -p 5007:80 -p 50007:443 -e ASPNETCORE_URLS="https://+;http://+" --env-file=.env -v %USERPROFILE%\.aspnet\https:/https/ history

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "History/History.csproj"
WORKDIR "/src/History"
RUN dotnet build "History.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "History.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "History.dll"]