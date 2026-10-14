#!/usr/bin/env bash
# Creates .env and adds keys missing from .env.example. Secrets and passwords (*_SECRET, *_PASSWORD, *_PASS)
# get a random value; existing values are never changed.
set -euo pipefail
cd "$(dirname "$0")/.."

touch .env
if [ -s .env ] && [ -n "$(tail -c1 .env)" ]; then echo >> .env; fi

while IFS= read -r line || [ -n "$line" ]; do
    case "$line" in ''|\#*) continue ;; esac
    key=${line%%=*}
    if ! grep -q "^$key=" .env; then
        case "$key" in
            *_SECRET) value=$(openssl rand -hex 32) ;;
            *_PASSWORD|*_PASS) value=$(openssl rand -hex 16) ;;
            *) value=${line#*=} ;;
        esac
        echo "$key=$value" >> .env
        echo "Added $key to .env"
    fi
done < .env.example
