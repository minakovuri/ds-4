#!/usr/bin/env bash

# Build container for BackendApi
docker build --rm --target backend-api -t ds-3/backend-api:latest -f ../Dockerfile ../

# Build container for Frontend
docker build --rm --target frontend -t ds-3/frontend:latest -f ../Dockerfile ../

# Build container for JobLogger
docker build --rm --target job-logger -t ds-3/job-logger -f ../Dockerfile ../