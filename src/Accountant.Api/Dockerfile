#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.100-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Accountant.Api/Accountant.Api.csproj", "src/Accountant.Api/"]
RUN dotnet restore "src/Accountant.Api/Accountant.Api.csproj"
COPY . .
WORKDIR "/src/src/Accountant.Api"
RUN dotnet build "Accountant.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accountant.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accountant.Api.dll"]