#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.100-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Account/Account.csproj", "src/Account/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/ToDoAssistant.Application/ToDoAssistant.Application.csproj", "src/ToDoAssistant.Application/"]
COPY ["src/CookingAssistant.Application/CookingAssistant.Application.csproj", "src/CookingAssistant.Application/"]
COPY ["src/Accountant.Application/Accountant.Application.csproj", "src/Accountant.Application/"]
COPY ["src/Weatherman.Application/Weatherman.Application.csproj", "src/Weatherman.Application/"]
COPY ["src/ToDoAssistant.Persistence/ToDoAssistant.Persistence.csproj", "src/ToDoAssistant.Persistence/"]
COPY ["src/CookingAssistant.Persistence/CookingAssistant.Persistence.csproj", "src/CookingAssistant.Persistence/"]
COPY ["src/Accountant.Persistence/Accountant.Persistence.csproj", "src/Accountant.Persistence/"]
COPY ["src/Weatherman.Persistence/Weatherman.Persistence.csproj", "src/Weatherman.Persistence/"]
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