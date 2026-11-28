FROM mcr.microsoft.com/dotnet/sdk:10.0.404-resolute@sha256:f685dd6b5a970778b53cead9a11cf54206c4b4fc3c97e4424a59b8bff9aa43d7 AS build

# Copy FileScan API
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/

# Build and publish
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:10.0.15-resolute-chiseled@sha256:ee8204b710f5e15d563176ee354364e29043d8e7125d69390521265cfb93fd72 AS final
EXPOSE 5200
WORKDIR /app
COPY --from=build /app_output .
USER $APP_UID

ENTRYPOINT ["dotnet", "Altinn.FileScan.dll"]
