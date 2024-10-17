# Keystone

KEYSTONE is a Student Lifecycle Management Application. Implemented with .NET CORE 2.1 with ELECTRON

# Status

DEVELOP : [![Build Status](https://dev.azure.com/genxas-dev/elite.keystone/_apis/build/status/KS-CI-DEVELOP?branchName=develop)](https://dev.azure.com/genxas-dev/elite.keystone/_build/latest?definitionId=19&branchName=develop)

# Prerequisite

- Visual Studio Code [Download link][Visual Studio Code Download Link] or Good Text Editor
- .NET Core 2.1.1 [Download link][.NET core Download Link]
- Node (Recommended Version) [Download link][Node Download Link]
- MS SQL SEVER (Windows User)
- Docker Engine
    MS SQL Linux Version (Mac User only)
- etc. (Maybe need more in the future)

# Getting Started

1. Clone the repo: git clone https://genxas-dev@dev.azure.com/genxas-dev/elite.keystone/_git/elite.keystone
2. Duplicate appsettings.Development.json.example and remove ".example"
3. For the first time, you are required to run database migration to update your local database by this command `dotnet ef database update`
4. Install node dependencies with command `npm install`
5. Run node prebuild script with command `npm rebuild node-sass`
6. Run node prebuild script with command `npm run prebuild`
7. Please check that you .NETCORE Env is set to Develop if not, please visits [How to set the hosting environment in ASP.NET Core](https://andrewlock.net/how-to-set-the-hosting-environment-in-asp-net-core/)
8. Run .NET CORE Application with command `dotnet run`

If you want to contribute README.MD please open a pull request! ~ENJOY~

[Visual Studio Code Download Link]: https://visualstudio.microsoft.com/downloads/
[.NET core Download Link]: https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.1.1-download.md
[Node Download Link]: https://nodejs.org/en/download/
