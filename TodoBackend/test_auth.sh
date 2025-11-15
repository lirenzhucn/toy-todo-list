#!/bin/bash

# Start the API in the background
cd /home/lirenzhu/Documents/job-search/ezra-take-home/TodoBackend/src/TodoBackend.API
dotnet run &
API_PID=$!

# Wait for the API to start
sleep 5

echo "Testing Authentication Endpoints..."
echo "==================================="

# Test registration endpoint
echo "1. Testing Registration Endpoint:"
curl -X POST http://localhost:5161/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "userName": "testuser",
    "password": "Test123!"
  }'

echo -e "\n\n2. Testing Login Endpoint:"
curl -X POST http://localhost:5161/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "password": "Test123!"
  }'

echo -e "\n\n3. Testing Todo Items Endpoint (should fail without token):"
curl -X GET http://localhost:5161/api/todoitems

echo -e "\n\nStopping API..."
kill $API_PID