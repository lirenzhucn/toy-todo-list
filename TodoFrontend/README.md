# Todo Frontend

Vue.js frontend for the Todo List Management application.

## Prerequisites

- Node.js (v18 or higher)
- npm or yarn
- Backend API running on http://localhost:5161

## Setup Instructions

### 1. Install Dependencies

```bash
cd frontend
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
frontend/
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
│   ├── router/          # Vue Router configuration
│   ├── main.ts          # App entry point
│   └── App.vue          # Root component
├── public/              # Static assets
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

## API Integration

The frontend integrates with the backend API through auto-generated TypeScript clients based on the OpenAPI specification. The API client provides:

- Type-safe API calls
- Automatic request/response serialization
- Error handling

## Development

### Code Style

The project uses ESLint and Prettier for code formatting and linting:

```bash
npm run lint    # Run ESLint
npm run format  # Run Prettier
```

### Testing

Run unit tests with Vitest:

```bash
npm run test:unit
```

## Production Deployment

1. Build the application:

   ```bash
   npm run build
   ```

2. The built files will be in the `dist/` directory

3. Serve the `dist/` directory with any static file server

## Troubleshooting

### API Connection Issues

- Ensure the backend API is running on http://localhost:5161
- Check the Vite proxy configuration in `vite.config.ts`
- Verify CORS settings on the backend

### Build Issues

- Clear node_modules and reinstall: `rm -rf node_modules && npm install`
- Check TypeScript compilation errors: `npm run build`

### Development Server Issues

- Check if port 3000 is available
- Try a different port: `npm run dev -- --port 3001`

