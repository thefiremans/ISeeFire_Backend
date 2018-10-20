#!/bin/bash
git pull
rm -rf ./deploy
dotnet publish -o ./deploy
