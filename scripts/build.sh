#!/bin/bash

# Calculate needed variables
HASH=`git rev-parse HEAD`
HASH=${HASH:0:10}

DIRECTORY=$(cd `dirname $0` && pwd)
echo $DIRECTORY

VERSION=5.0.1

echo "Preparing deploy for Git commit: $HASH"
echo "Working directory: $DIRECTORY"
cd $DIRECTORY

# Build
dotnet clean -c release ../src/Cubes.Core/Cubes.Core.csproj
dotnet build -c release ../src/Cubes.Core/Cubes.Core.csproj -p:VersionPrefix=$VERSION --version-suffix "$HASH" -p:InformationalVersion=$VERSION-$HASH
dotnet clean -c release ../src/Cubes.Api/Cubes.Api.csproj
dotnet build -c release ../src/Cubes.Api/Cubes.Api.csproj -p:VersionPrefix=$VERSION --version-suffix "$HASH" -p:InformationalVersion=$VERSION-$HASH
dotnet clean -c release ../src/Cubes.Host/Cubes.Host.csproj
dotnet build -c release ../src/Cubes.Host/Cubes.Host.csproj -p:VersionPrefix=$VERSION --version-suffix "$HASH" -p:InformationalVersion=$VERSION-$HASH


# Publish
mkdir ../tmp
cd ../tmp
rm -rf *
dotnet publish --no-build ../src/Cubes.Host/Cubes.Host.csproj -o $DIRECTORY/../tmp/Cubes-$HASH -c release -p:VersionPrefix=$VERSION --version-suffix "$HASH"

# Create zip
zip -r Cubes-$HASH.zip .
mv Cubes-$HASH.zip ../deploy

# Cleanup
rm -rf ../tmp
