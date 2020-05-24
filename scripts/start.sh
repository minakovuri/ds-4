#!/usr/bin/env bash

# To parse JSON params, you should run `sudo apt install jq`
JQ_OK=$(dpkg-query -W --showformat='${Status}\n' jq | grep "install ok installed")
if [ "" == "$JQ_OK" ]; then
    echo " --------- INSTALLING JSON PARSER ---------- "
    sudo apt install jq
fi

BACKEND_API_HOST=$( jq .BackendApi.host ../config/config.json | tr -d '"' )
BACKEND_API_PORT=$( jq .BackendApi.port ../config/config.json )

FRONTEND_PORT=$( jq .Frontend.port ../config/config.json )

REDIS_HOST=$( jq .Redis.host ../config/config.json | tr -d '"' )
REDIS_PORT=$( jq .Redis.port ../config/config.json )

NATS_HOST=$( jq .Nats.host ../config/config.json | tr -d '"' )
NATS_PORT=$( jq .Nats.port ../config/config.json )

echo " --------- Run Nuts  ---------------"
docker run --rm -d -p ${NATS_PORT}:4222 --name nats nats

echo " ---------  Run Redis  -------------"
docker run --rm -d -p ${REDIS_PORT}:6379 --name redis redis

echo " --------- Run BackendApi ------------ "
docker run -e ASPNETCORE_URLS=http://+:${BACKEND_API_PORT} -e NATS_HOST=${NATS_HOST} -e NATS_PORT=${NATS_PORT} -e REDIS_HOST=${REDIS_HOST} -e REDIS_PORT=${REDIS_PORT} --rm -d -p ${BACKEND_API_PORT}:${BACKEND_API_PORT} --name backend-api --link=nats --link=redis ds-4/backend-api

echo " ---------  Run Frontend   ------------ "
docker run -e ASPNETCORE_URLS=http://+:${FRONTEND_PORT} -e BACKEND_API_HOST=${BACKEND_API_HOST} -e BACKEND_API_PORT=${BACKEND_API_PORT} --rm -d -p ${FRONTEND_PORT}:${FRONTEND_PORT} --name frontend --link=backend-api:${BACKEND_API_HOST} ds-4/frontend

echo " --------- Run JobLogger ------------ "
docker run -e NATS_HOST=${NATS_HOST} -e NATS_PORT=${NATS_PORT} -e REDIS_HOST=${REDIS_HOST} -e REDIS_PORT=${REDIS_PORT} --rm -d --name job-logger --link=nats --link=redis ds-4/job-logger

echo " --------- Run TextRankCalc ------------ "
docker run -e NATS_HOST=${NATS_HOST} -e NATS_PORT=${NATS_PORT} -e REDIS_HOST=${REDIS_HOST} -e REDIS_PORT=${REDIS_PORT} --rm -d --name text-rank-calc --link=nats --link=redis ds-4/text-rank-calc