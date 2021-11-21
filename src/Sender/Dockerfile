#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0.100-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Sender/Sender.csproj", "src/Sender/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
RUN dotnet restore "src/Sender/Sender.csproj"
COPY . .
WORKDIR "/src/src/Sender"
RUN dotnet build "Sender.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sender.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sender.dll"]