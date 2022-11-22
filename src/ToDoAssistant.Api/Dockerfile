#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.100-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/ToDoAssistant.Api/ToDoAssistant.Api.csproj", "src/ToDoAssistant.Api/"]
RUN dotnet restore "src/ToDoAssistant.Api/ToDoAssistant.Api.csproj"
COPY . .
WORKDIR "/src/src/ToDoAssistant.Api"
RUN dotnet build "ToDoAssistant.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ToDoAssistant.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoAssistant.Api.dll"]