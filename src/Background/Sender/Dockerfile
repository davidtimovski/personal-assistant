FROM mcr.microsoft.com/dotnet/aspnet:7.0.5-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Cs.targets", "./"]
COPY ["src/Background/Sender/Sender.csproj", "src/Background/Sender/"]
RUN dotnet restore "src/Background/Sender/Sender.csproj"
COPY . .
WORKDIR "/src/src/Background/Sender"
RUN dotnet build "Sender.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sender.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sender.dll"]