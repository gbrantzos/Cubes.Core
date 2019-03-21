#!/bin/bash

# Calculate needed variables
HASH=`git rev-parse HEAD`
HASH=${HASH:0:10}

DIRECTORY=$(cd `dirname $0` && pwd)
echo $DIRECTORY

echo "Preparing deploy for Git commit: $HASH"
echo "Working directory: $DIRECTORY"
cd $DIRECTORY

# Build
dotnet build ../src/Cubes.Core/Cubes.Core.csproj /p:InformationalVersion=5.0.1-$HASH
dotnet build ../src/Cubes.Api/Cubes.Api.csproj /p:InformationalVersion=5.0.1-$HASH
dotnet build ../src/Cubes.Host/Cubes.Host.csproj /p:InformationalVersion=5.0.1-$HASH

# Publish
mkdir ../tmp
cd ../tmp
rm -rf *
dotnet publish ../src/Cubes.Host/Cubes.Host.csproj -o $DIRECTORY/../tmp/Cubes-$HASH -c Release

# Create zip
zip -r Cubes-$HASH.zip .
mv Cubes-$HASH.zip ../deploy

# Cleanup
rm -rf ../tmp

