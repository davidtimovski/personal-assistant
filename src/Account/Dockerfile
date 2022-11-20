#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.100-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Account/Account.csproj", "src/Account/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Utility/Utility.fsproj", "src/Utility/"]
COPY ["src/Persistence/Persistence.csproj", "src/Persistence/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Account/Account.csproj"
COPY . .
WORKDIR "/src/src/Account"
RUN dotnet build "Account.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Account.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Account.dll"]