#!/bin/bash

if [ -d "/log" ] 
then
    echo "/log exists"
    logdir="/log"
else
    echo "'/log' directory does not exist. Defaulting to '/app'."
    logdir="/app"
fi

exec > >(tee "$logdir/console.log") 2>&1

exec /app/Cloud-ShareSync.SimpleBackup