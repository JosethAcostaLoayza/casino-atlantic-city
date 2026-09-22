FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY src/Control/Control.csproj src/Control/
COPY src/Control.Application/Control.Application.csproj src/Control.Application/
COPY src/Control.Domain/Control.Domain.csproj src/Control.Domain/
COPY src/Control.Infrastructure/Control.Infrastructure.csproj src/Control.Infrastructure/

RUN dotnet restore src/Control/Control.csproj

COPY src/ src/

RUN dotnet publish src/Control/Control.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Control.dll"]