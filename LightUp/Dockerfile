FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["LightUp/LightUp.csproj", "LightUp/"]
RUN dotnet restore "LightUp/LightUp.csproj"
COPY . .
WORKDIR "/src/LightUp"
RUN dotnet build "LightUp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LightUp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LightUp.dll"]