FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/Authentication/Authentication.csproj", "src/Authentication/"]

RUN dotnet restore "src/Authentication/Authentication.csproj"

COPY . .

RUN dotnet publish "src/Authentication/Authentication.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Authentication.dll"]