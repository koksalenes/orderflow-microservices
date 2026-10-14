#!/bin/sh
set -e

# Runs once, on first container initialization (empty data volume), via
# /docker-entrypoint-initdb.d. Every service owns its own database; the list
# comes from APP_DATABASES in docker-compose.yml.
for db in $APP_DATABASES; do
    echo "Creating database $db"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres \
        -c "CREATE DATABASE \"$db\";"
done
