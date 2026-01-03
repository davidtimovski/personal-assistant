FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Fs.targets", "./"]
COPY ["src/Weatherman/Weatherman.Api/Weatherman.Api.fsproj", "src/Weatherman/Weatherman.Api/"]
RUN dotnet restore "src/Weatherman/Weatherman.Api/Weatherman.Api.fsproj"
COPY . .
WORKDIR "/src/src/Weatherman/Weatherman.Api"
RUN dotnet build "Weatherman.Api.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Weatherman.Api.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Weatherman.Api.dll"]