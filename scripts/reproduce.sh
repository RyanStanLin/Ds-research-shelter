#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet build src/DhhResearch/DhhResearch.csproj --configuration Release --nologo
app=src/DhhResearch/bin/Release/net10.0/DhhResearch.dll
dotnet "$app" validate .
dotnet "$app" analyze .
dotnet "$app" summarize .
dotnet "$app" audit-data .
