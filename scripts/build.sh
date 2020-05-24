#!/usr/bin/env bash

echo " ----- Build container for BackendApi -----"
docker build --rm --target backend-api -t ds-4/backend-api:latest -f ../Dockerfile ../

echo "----- Build container for Frontend -----"
docker build --rm --target frontend -t ds-4/frontend:latest -f ../Dockerfile ../

echo "---- Build container for JobLogger -----"
docker build --rm --target job-logger -t ds-4/job-logger -f ../Dockerfile ../

echo "---- Build container for TextRankCalc -----"
docker build --rm --target text-rank-calc -t ds-4/text-rank-calc -f ../Dockerfile ../

echo " ----------------- Results -----------------"
echo " ---- BackendApi container building - success ---- "
echo " ----- Frontend container building - success -----"
echo " ----- JobLogger container building - success -----"
echo " ----- TextRankCalc container building - success -----"