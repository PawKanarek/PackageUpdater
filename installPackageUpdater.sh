#!/bin/zsh

curl -o PackageUpdater -L https://github.com/PawKanarek/PackageUpdater/releases/download/1.2-mac/PackageUpdater

cp PackageUpdater /usr/local/bin/PackageUpdater

rm PackageUpdater

chmod +x /usr/local/bin/PackageUpdater

PackageUpdater -h
