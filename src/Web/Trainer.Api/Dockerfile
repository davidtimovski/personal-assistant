#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Web/Trainer.Api/Trainer.Api.fsproj", "src/Web/Trainer.Api/"]
RUN dotnet restore "src/Web/Trainer.Api/Trainer.Api.fsproj"
COPY . .
WORKDIR "/src/src/Web/Trainer.Api"
RUN dotnet build "Trainer.Api.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Trainer.Api.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Trainer.Api.dll"]