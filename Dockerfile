FROM mcr.microsoft.com/dotnet/sdk:9.0.203-alpine3.21@sha256:33be1326b4a2602d08e145cf7e4a8db4b243db3cac3bdec42e91aef930656080 AS build

# Copy event backend
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/


# Build and publish
RUN dotnet build Altinn.FileScan.csproj -c Release -o /app_output
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:9.0.4-alpine3.21@sha256:3fce6771d84422e2396c77267865df61174a3e503c049f1fe242224c012fde65 AS final
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
