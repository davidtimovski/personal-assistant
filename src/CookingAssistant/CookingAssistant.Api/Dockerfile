FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Cs.targets", "./"]
COPY ["src/CookingAssistant/CookingAssistant.Api/CookingAssistant.Api.csproj", "src/CookingAssistant/CookingAssistant.Api/"]
RUN dotnet restore "src/CookingAssistant/CookingAssistant.Api/CookingAssistant.Api.csproj"
COPY . .
WORKDIR "/src/src/CookingAssistant/CookingAssistant.Api"
RUN dotnet build "CookingAssistant.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CookingAssistant.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CookingAssistant.Api.dll"]