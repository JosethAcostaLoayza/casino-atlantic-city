FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY src/Notificaciones/Notificaciones.csproj src/Notificaciones/

RUN dotnet restore src/Notificaciones/Notificaciones.csproj

COPY src/ src/

RUN dotnet publish src/Notificaciones/Notificaciones.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Notificaciones.dll"]