#!/bin/bash

# Start the API in the background
cd /home/lirenzhu/Documents/job-search/ezra-take-home/TodoBackend/src/TodoBackend.API
dotnet run &
API_PID=$!

# Wait for the API to start
sleep 5

echo "Testing Full Authentication Flow with Todo Items..."
echo "==================================================="

# Test registration endpoint
echo "1. Registering new user:"
REGISTER_RESPONSE=$(curl -s -X POST http://localhost:5161/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testflow@example.com",
    "userName": "testflow",
    "password": "Test123!"
  }')

echo "$REGISTER_RESPONSE"

# Extract token from registration response
TOKEN=$(echo "$REGISTER_RESPONSE" | grep -o '"token":"[^"]*' | cut -d'"' -f4)

echo -e "\n\n2. Extracted Token: $TOKEN"

# Test getting todo items with token
echo -e "\n\n3. Getting todo items with authentication:"
curl -X GET http://localhost:5161/api/todoitems \
  -H "Authorization: Bearer $TOKEN"

# Test creating a todo item
echo -e "\n\n4. Creating a new todo item:"
CREATE_RESPONSE=$(curl -s -X POST http://localhost:5161/api/todoitems \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Todo Item",
    "description": "This is a test todo item",
    "isComplete": false
  }')

echo "$CREATE_RESPONSE"

# Extract todo item ID from creation response
TODO_ID=$(echo "$CREATE_RESPONSE" | grep -o '"id":[0-9]*' | cut -d':' -f2)

echo -e "\n\n5. Getting specific todo item:"
curl -X GET "http://localhost:5161/api/todoitems/$TODO_ID" \
  -H "Authorization: Bearer $TOKEN"

# Test updating the todo item
echo -e "\n\n6. Updating todo item:"
curl -X PUT "http://localhost:5161/api/todoitems/$TODO_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": '$TODO_ID',
    "title": "Updated Test Todo Item",
    "description": "This is an updated test todo item",
    "isComplete": true
  }'

# Test getting all todo items again
echo -e "\n\n7. Getting all todo items after update:"
curl -X GET http://localhost:5161/api/todoitems \
  -H "Authorization: Bearer $TOKEN"

# Test deleting the todo item
echo -e "\n\n8. Deleting todo item:"
curl -X DELETE "http://localhost:5161/api/todoitems/$TODO_ID" \
  -H "Authorization: Bearer $TOKEN"

# Test getting todo items without token (should fail)
echo -e "\n\n9. Testing unauthorized access (should fail):"
curl -X GET http://localhost:5161/api/todoitems

echo -e "\n\nStopping API..."
kill $API_PID