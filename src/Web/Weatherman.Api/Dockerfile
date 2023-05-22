FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Fs.targets", "./"]
COPY ["src/Web/Weatherman.Api/Weatherman.Api.fsproj", "src/Web/Weatherman.Api/"]
RUN dotnet restore "src/Web/Weatherman.Api/Weatherman.Api.fsproj"
COPY . .
WORKDIR "/src/src/Web/Weatherman.Api"
RUN dotnet build "Weatherman.Api.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Weatherman.Api.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Weatherman.Api.dll"]