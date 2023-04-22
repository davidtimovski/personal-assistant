FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/Web/Accountant.Api/Accountant.Api.fsproj", "src/Web/Accountant.Api/"]
RUN dotnet restore "src/Web/Accountant.Api/Accountant.Api.fsproj"
COPY . .
WORKDIR "/src/src/Web/Accountant.Api"
RUN dotnet build "Accountant.Api.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accountant.Api.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accountant.Api.dll"]