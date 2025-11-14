# Todo Frontend

Vue.js frontend for the Todo List Management application.

## Setup Instructions

### 1. Install Dependencies

```bash
npm install --include=dev
```

### 2. Generate API Client

Generate TypeScript API client from the OpenAPI specification:

```bash
npm run generate-api
```

This will create the API client in `src/api/` based on the OpenAPI spec in the parent directory.

### 3. Start Development Server

```bash
npm run dev
```

The application will be available at http://localhost:3000

### 4. Build for Production

```bash
npm run build
```

## Project Structure

```
TodoFrontend/
├── src/
│   ├── api/              # Generated API client
│   ├── components/       # Reusable Vue components
│   │   ├── TodoTable.vue # Todo list table
│   │   └── TodoForm.vue  # Add/edit todo form
│   ├── views/           # Page components
│   │   └── TodoList.vue # Main todo list page
│   ├── stores/          # Pinia stores
│   │   └── todo.ts      # Todo state management
│   ├── types/           # TypeScript types
│   │   └── todo.ts      # Todo-related types
│   ├── main.ts          # App entry point
│   └── App.vue          # Root component
└── package.json
```

## Features

- **Todo Management**: Create, read, update, and delete todos
- **Status Tracking**: Mark todos as complete/incomplete
- **Date Scheduling**: Schedule and set due dates for todos
- **Filtering**: View todos by status (all, incomplete, complete, overdue)
- **Responsive Design**: Works on desktop and mobile devices

## Technologies Used

- **Vue 3** with Composition API
- **TypeScript** for type safety
- **Vite** for fast development and building
- **Element Plus** for UI components
- **Pinia** for state management
- **Axios** for HTTP requests
- **Vue Router** for navigation
