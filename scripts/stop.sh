#!/usr/bin/env bash

echo " ----- Stop Frontend ------ "
docker stop frontend

echo " ----- Stop BackendApi ------ "
docker stop backend-api

echo " ---- Stop Redis --------"
docker stop redis

echo " --- Stop Nats ----------"
docker stop nats

echo " --- Stop JobLogger ----"
docker stop job-logger