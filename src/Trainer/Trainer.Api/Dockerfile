FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Fs.targets", "./"]
COPY ["src/Trainer/Trainer.Api/Trainer.Api.fsproj", "src/Trainer/Trainer.Api/"]
RUN dotnet restore "src/Trainer/Trainer.Api/Trainer.Api.fsproj"
COPY . .
WORKDIR "/src/src/Trainer/Trainer.Api"
RUN dotnet build "Trainer.Api.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Trainer.Api.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Trainer.Api.dll"]