FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Cs.targets", "./"]
COPY ["src/Core/PersonalAssistant.Web/PersonalAssistant.Web.csproj", "src/Core/PersonalAssistant.Web/"]
RUN dotnet restore "src/Core/PersonalAssistant.Web/PersonalAssistant.Web.csproj"
COPY . .
WORKDIR "/src/src/Core/PersonalAssistant.Web"
RUN dotnet build "PersonalAssistant.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalAssistant.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalAssistant.Web.dll"]