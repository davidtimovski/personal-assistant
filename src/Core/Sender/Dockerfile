FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Cs.targets", "./"]
COPY ["src/Core/Sender/Sender.csproj", "src/Core/Sender/"]
RUN dotnet restore "src/Core/Sender/Sender.csproj"
COPY . .
WORKDIR "/src/src/Core/Sender"
RUN dotnet build "Sender.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sender.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sender.dll"]