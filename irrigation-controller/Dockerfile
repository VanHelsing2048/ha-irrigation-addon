FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY src/IrrigationController/IrrigationController.csproj ./IrrigationController/
RUN dotnet restore ./IrrigationController/IrrigationController.csproj

COPY src/IrrigationController ./IrrigationController
RUN dotnet publish ./IrrigationController/IrrigationController.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV IRRIGATION_CONFIG_PATH=/data/irrigation.json
ENV IRRIGATION_STATE_PATH=/data/state.json

COPY --from=build /app/publish .
COPY run.sh /run.sh
RUN chmod +x /run.sh

CMD ["/run.sh"]
