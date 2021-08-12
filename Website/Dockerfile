#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# docker build -t website:latest -f . ..
# docker run --rm -it -p 5008:80 -e ASPNETCORE_URLS="http://+;" website

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5008

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Website/Website.csproj"
WORKDIR "/src/Website"
RUN dotnet build "Website.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Website.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Website.dll"]