#!/usr/bin/env bash
# Initialize the ContosoOrders database inside the local docker container.
# Idempotent: schema scripts use IF NOT EXISTS / CREATE OR ALTER, seed is guarded.
set -euo pipefail
cd "$(dirname "$0")"

SA_PASSWORD="${MSSQL_SA_PASSWORD:-ContosoDev!Passw0rd}"
SQLCMD=(docker exec -i contoso-mssql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$SA_PASSWORD" -b)

echo "Creating database..."
"${SQLCMD[@]}" -Q "IF DB_ID('ContosoOrders') IS NULL CREATE DATABASE ContosoOrders;"

run_dir() {
  local dir="$1"
  for f in $(ls "$dir"/*.sql 2>/dev/null | sort); do
    echo "Applying $f"
    "${SQLCMD[@]}" -d ContosoOrders -i /dev/stdin < "$f"
  done
}

run_dir schema
run_dir seed
run_dir migrations

echo "Database ready."
