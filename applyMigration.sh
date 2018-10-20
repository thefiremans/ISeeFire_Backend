#!/bin/sh

echo 'removing db'
rm isf.db

echo 'running migration'
dotnet ef database update

