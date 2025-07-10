FROM mcr.microsoft.com/dotnet/sdk:9.0.302-alpine3.22@sha256:35c77bbcd86b3153f9d7d6b2e88ae99d300e5a570cf2827501c0ba8b0aacdf08 AS build

# Copy event backend
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/


# Build and publish
RUN dotnet build Altinn.FileScan.csproj -c Release -o /app_output
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:9.0.7-alpine3.22@sha256:53867b9ebb86beab644de47226867d255d0360e38324d2afb3ff4d2f2933e33f AS final
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
