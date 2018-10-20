#!/bin/bash

echo 'pulling changes'
git pull
echo 'migrating'
./applyMigration.sh
echo 'removing deploy'
rm -rf ./deploy
echo 'running dotnet publish'
dotnet publish -o ./deploy
