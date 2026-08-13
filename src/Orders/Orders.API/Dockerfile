# Fase base: se usa en producción / modo normal de ejecución.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Fase de compilación.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Orders/Orders.API/Orders.API.csproj", "src/Orders/Orders.API/"]
COPY ["src/BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "src/BuildingBlocks/BuildingBlocks/"]
RUN dotnet restore "./src/Orders/Orders.API/Orders.API.csproj"
COPY . .
WORKDIR "/src/src/Orders/Orders.API"
RUN dotnet build "./Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de publicación.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Orders.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final: imagen ligera solo con el binario publicado.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.API.dll"]
