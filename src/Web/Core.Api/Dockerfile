#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.100-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Web/Core.Api/Core.Api.csproj", "src/Web/Core.Api/"]
RUN dotnet restore "src/Web/Core.Api/Core.Api.csproj"
COPY . .
WORKDIR "/src/src/Web/Core.Api"
RUN dotnet build "Core.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.Api.dll"]