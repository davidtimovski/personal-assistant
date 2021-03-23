#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/Worker/Worker.csproj", "src/Worker/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Utility/Utility.fsproj", "src/Utility/"]
COPY ["src/Persistence/Persistence.csproj", "src/Persistence/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Worker/Worker.csproj"
COPY . .
WORKDIR "/src/src/Worker"
RUN dotnet build "Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Worker.dll"]