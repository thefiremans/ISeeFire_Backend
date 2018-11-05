#!/bin/bash
cd ISeeFire_Backend

echo 'stopping service'
sudo systemctl stop kestrel-helloapp.service

./deploy.sh

echo 'restarting service'
sudo systemctl restart kestrel-helloapp.service

