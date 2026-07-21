#!/bin/bash
set -e

# Create databases for each microservice
echo "Creating HR Analytics databases..."

PGPASSWORD=$POSTGRES_PASSWORD psql -v ON_ERROR_STOP=1 -U $POSTGRES_USER <<-EOSQL
    -- Create databases for each service
    CREATE DATABASE hr_identity;
    CREATE DATABASE hr_employee;
    CREATE DATABASE hr_performance;
    CREATE DATABASE hr_attendance;
    CREATE DATABASE hr_payroll;
    
    -- Future databases for other services
    CREATE DATABASE hr_analytics;
    CREATE DATABASE hr_notification;
    CREATE DATABASE hr_audit;
    
    -- Log creation
    SELECT now(), 'Database creation completed successfully';
EOSQL

echo "Database initialization completed!"
