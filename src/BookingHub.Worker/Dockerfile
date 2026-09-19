FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/BookingHub.Worker/BookingHub.Worker.csproj
RUN dotnet publish src/BookingHub.Worker/BookingHub.Worker.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookingHub.Worker.dll"]
