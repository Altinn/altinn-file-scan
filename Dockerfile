FROM mcr.microsoft.com/dotnet/sdk:10.0.403-resolute@sha256:d32b07e1e85ea9737338e49451b2804f861e18e8507feaf3ef7b20a1568569c2 AS build

# Copy FileScan API
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/

# Build and publish
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-resolute-chiseled@sha256:9fd8f06517e807d3cf404f107ccca7a0d46a2d288aa088f41a7e532948ab43ad AS final
EXPOSE 5200
WORKDIR /app
COPY --from=build /app_output .
USER $APP_UID

ENTRYPOINT ["dotnet", "Altinn.FileScan.dll"]
