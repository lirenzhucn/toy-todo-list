# Todo Frontend

Vue.js frontend for the Todo List Management application with user authentication.

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

## Technologies Used

- **Vue 3** with Composition API
- **TypeScript** for type safety
- **Vite** for fast development and building
- **Element Plus** for UI components
- **Pinia** for state management
- **Axios** for HTTP requests
- **Vue Router** for navigation
- **OpenAPI Generator** for API client generation
