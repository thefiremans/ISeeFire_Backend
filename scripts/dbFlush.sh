#!/bin/bash
cd ISeeFire_Backend

echo 'stopping service'
sudo systemctl stop kestrel-helloapp.service

echo 'refreshing db'
cp isf.db2 isf.db

echo 'restarting service'
sudo systemctl restart kestrel-helloapp.service

