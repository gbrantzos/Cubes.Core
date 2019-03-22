#!/bin/bash

# Populate needed variables
HASH=`git rev-parse HEAD`
HASH=${HASH:0:10}

DIRECTORY=$(cd `dirname $0` && pwd)
echo $DIRECTORY

VERSION=5.0.1

echo "Preparing deploy for Git commit: $HASH"
echo "Working directory: $DIRECTORY"
cd $DIRECTORY

# Check for uncommited files
if ! [ -z "$(git status --porcelain)" ]; then
    echo There are uncommited files on workspace, aborting!
    exit 1
fi

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
dotnet publish --no-build ../src/Cubes.Host/Cubes.Host.csproj -o $DIRECTORY/../tmp/Cubes-$HASH -c release

# Create zip
tar -czvf Cubes-$HASH.tar.gz Cubes-$HASH/*
cd ..
mkdir -p deploy
mv tmp/Cubes-$HASH.tar.gz deploy

# Cleanup
rm -rf tmp
exit 0