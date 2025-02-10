# Altinn FileScan

Microservice that performs asynchronous malware scanning of files uploaded to Altinn 3.
Documentation for setting up filescan in an app: https://docs.altinn.studio/app/development/configuration/filescan/

## Build status
[![Filescan build status](https://dev.azure.com/brreg/altinn-studio/_apis/build/status/altinn-platform/filescan-master?label=altinn/filescan)](https://dev.azure.com/brreg/altinn-studio/_build/latest?definitionId=405)

## Getting Started

These instructions will get you a copy of the filescan component up and running on your machine for development and testing purposes.

### Prerequisites

1. [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Newest [Git](https://git-scm.com/downloads)
3. A code editor - we like [Visual Studio Code](https://code.visualstudio.com/download)
   - Install [Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions). You can also install the [Azure Tools extension pack](https://marketplace.visualstudio.com/items?itemName=ms-vscode.vscode-node-azure-pack), which is recommended for working with Azure resources.
   - Also install [recommended extensions](https://code.visualstudio.com/docs/editor/extension-marketplace#_workspace-recommended-extensions) (e.g. [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp))
5. [Podman](https://podman.io/) or another container tool such as Docker Desktop
6. Install [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage#install-azurite)
7. Install [Azure Functions Core Tool](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-powershell#install-the-azure-functions-core-tools)
8. Follow the readme to run [Muescheli](https://github.com/Altinn/muescheli) locally

### Cloning the application
Clone Altinn FileScan repo and navigate to the folder.


```bash
git clone https://github.com/Altinn/altinn-file-scan
cd altinn-file-scan
```

### Running the application

Start Altinn FileScan application 
```bash
cd src/Altinn.FileScan
dotnet run
```

The filescan solution is now available locally at http://localhost:5200/.
To access swagger use http://localhost:5200/filescan/swagger.

### Running functions

- [Start Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio%2Cblob-storage#run-azurite)
  
Start Altinn FileScan Functions
```bash
cd src/Altinn.FileScan.Functions
func start
```