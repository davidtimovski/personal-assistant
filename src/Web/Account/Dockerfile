FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Web/Account/Account.csproj", "src/Web/Account/"]
RUN dotnet restore "src/Web/Account/Account.csproj"
COPY . .
WORKDIR "/src/src/Web/Account"
RUN dotnet build "Account.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Account.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Account.dll"]