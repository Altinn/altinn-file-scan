FROM mcr.microsoft.com/dotnet/sdk:10.0.403-resolute@sha256:667f3cee6f24afb202d3e32c7941e3800bbac57dd70bb68f89e52a6245aff88f AS build

# Copy FileScan API
COPY src/Altinn.FileScan ./Altinn.FileScan
WORKDIR Altinn.FileScan/

# Build and publish
RUN dotnet publish Altinn.FileScan.csproj -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-resolute-chiseled@sha256:6018ff839e76c1bc9aa58d60c2ba8e596061363c88d8f4f7e45d1c19b6205eaf AS final
EXPOSE 5200
WORKDIR /app
COPY --from=build /app_output .
USER $APP_UID

ENTRYPOINT ["dotnet", "Altinn.FileScan.dll"]
