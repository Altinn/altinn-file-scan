FROM mcr.microsoft.com/dotnet/sdk:10.0.401-resolute@sha256:d8051d88928fb40f74c9aca6896a57851cdced277f4102e1c5aa6d4983e27bad AS build

# Copy FileScan API
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/

# Build and publish
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:10.0.13-resolute-chiseled@sha256:c721423a06bad868eeed8c6b0037463b3007d3d929f4cb333ac8e87f8a8f8101 AS final
EXPOSE 5200
WORKDIR /app
COPY --from=build /app_output .
USER $APP_UID

ENTRYPOINT ["dotnet", "Altinn.FileScan.dll"]
