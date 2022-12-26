#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.100-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Web/ClientLogger/ClientLogger.fsproj", "src/Web/ClientLogger/"]
RUN dotnet restore "src/Web/ClientLogger/ClientLogger.fsproj"
COPY . .
WORKDIR "/src/src/Web/ClientLogger"
RUN dotnet build "ClientLogger.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClientLogger.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientLogger.dll"]