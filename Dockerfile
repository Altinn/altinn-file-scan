FROM mcr.microsoft.com/dotnet/sdk:9.0.305-alpine3.22@sha256:c802e10f4676d89b4d0bc636ffded77a7a1d17359544764a62705146743b8ca0 AS build

# Copy event backend
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/


# Build and publish
RUN dotnet build Altinn.FileScan.csproj -c Release -o /app_output
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:9.0.9-alpine3.22@sha256:d2bef2c7ecb618e02c5e2f29b448be0aca1f82993800066c3622e7cabfca9ead AS final
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
