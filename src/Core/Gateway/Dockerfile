FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Cs.targets", "./"]
COPY ["src/Core/Gateway/Gateway.csproj", "src/Core/Gateway/"]
RUN dotnet restore "src/Core/Gateway/Gateway.csproj"
COPY . .
WORKDIR "/src/src/Core/Gateway"
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]