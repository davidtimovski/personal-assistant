FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PersonalAssistant.targets", "PersonalAssistant.Fs.targets", "./"]
COPY ["src/Accountant/Accountant.Api/Accountant.Api.fsproj", "src/Accountant/Accountant.Api/"]
RUN dotnet restore "src/Accountant/Accountant.Api/Accountant.Api.fsproj"
COPY . .
WORKDIR "/src/src/Accountant/Accountant.Api"
RUN dotnet build "Accountant.Api.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accountant.Api.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accountant.Api.dll"]