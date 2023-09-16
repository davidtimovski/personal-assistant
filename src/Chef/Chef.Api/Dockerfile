FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Cs.targets", "./"]
COPY ["src/Chef/Chef.Api/Chef.Api.csproj", "src/Chef/Chef.Api/"]
RUN dotnet restore "src/Chef/Chef.Api/Chef.Api.csproj"
COPY . .
WORKDIR "/src/src/Chef/Chef.Api"
RUN dotnet build "Chef.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chef.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chef.Api.dll"]