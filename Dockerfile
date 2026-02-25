FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.23@sha256:ebdd46c1a72c10bb992c276e5e6afc6e245757bd1a295d7f0870b266e11c306e AS build

# Copy event backend
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/


# Build and publish
RUN dotnet build Altinn.FileScan.csproj -c Release -o /app_output
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.23@sha256:c60d76bf35b268f7bab7d5f4e5405c89dcc45616453004e89baea303677ca635 AS final
EXPOSE 5200
WORKDIR /app
COPY --from=build /app_output .

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet
# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry

ENTRYPOINT ["dotnet", "Altinn.FileScan.dll"]
