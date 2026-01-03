FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "PersonalAssistant.targets", "PersonalAssistant.Cs.targets", "./"]
COPY ["src/Core/Core.Api/Core.Api.csproj", "src/Core/Core.Api/"]
RUN dotnet restore "src/Core/Core.Api/Core.Api.csproj"
COPY . .
WORKDIR "/src/src/Core/Core.Api"
RUN dotnet build "Core.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.Api.dll"]