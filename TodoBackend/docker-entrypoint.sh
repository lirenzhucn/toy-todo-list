#!/bin/bash
set -e

echo "Starting TodoBackend application..."

# Ensure the database directory exists
mkdir -p /app/data

# Check if SQLite is available
if command -v sqlite3 &> /dev/null; then
    echo "SQLite3 is available"
    
    # Check if database file exists, if not create it
    if [ ! -f "/app/data/Todo.db" ]; then
        echo "Creating new database file at /app/data/Todo.db"
        touch /app/data/Todo.db
    fi
    
    # Make sure the database file is writable
    chmod 664 /app/data/Todo.db
else
    echo "SQLite3 not found, proceeding without SQLite checks"
fi

# Set proper permissions for the data directory
chown -R appuser:appuser /app/data

echo "Database initialization complete"
echo "Starting .NET application..."

# Execute the main command (passed as arguments to this script)
exec "$@"