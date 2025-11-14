import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { TodoItem } from '@/types/todo'
import { TodoApiClient } from '@/api'

export const useTodoStore = defineStore('todo', () => {
  const todos = ref<TodoItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const apiClient = new TodoApiClient({
    // this trick uses the dynamic vite api url to point the client object to the correct backend url
    BASE: import.meta.env.VITE_API_BASE_URL || '',
  })

  // Getters
  const completedTodos = computed(() => todos.value.filter(todo => todo.isComplete))
  const incompleteTodos = computed(() => todos.value.filter(todo => !todo.isComplete))
  const overdueTodos = computed(() => {
    const now = new Date()
    return todos.value.filter(todo => 
      todo.dueDateTime && new Date(todo.dueDateTime) < now && !todo.isComplete
    )
  })

  // Actions
  async function fetchTodos() {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.todoBackendApi.getApiTodoitems()
      todos.value = response
    } catch (err) {
      error.value = 'Failed to fetch todos'
      console.error('Error fetching todos:', err)
    } finally {
      loading.value = false
    }
  }

  async function addTodo(todo: Omit<TodoItem, 'id'>) {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.todoBackendApi.postApiTodoitems(todo)
      todos.value.push(response)
    } catch (err) {
      error.value = 'Failed to add todo'
      console.error('Error adding todo:', err)
    } finally {
      loading.value = false
    }
  }

  async function updateTodo(id: number, todo: TodoItem) {
    loading.value = true
    error.value = null
    try {
      const response = await apiClient.todoBackendApi.putApiTodoitems(id, todo)
      const index = todos.value.findIndex(t => t.id === id)
      if (index !== -1) {
        todos.value[index] = response
      }
    } catch (err) {
      error.value = 'Failed to update todo'
      console.error('Error updating todo:', err)
    } finally {
      loading.value = false
    }
  }

  async function deleteTodo(id: number) {
    loading.value = true
    error.value = null
    try {
      await apiClient.todoBackendApi.deleteApiTodoitems(id)
      todos.value = todos.value.filter(todo => todo.id !== id)
    } catch (err) {
      error.value = 'Failed to delete todo'
      console.error('Error deleting todo:', err)
    } finally {
      loading.value = false
    }
  }

  return {
    todos,
    loading,
    error,
    completedTodos,
    incompleteTodos,
    overdueTodos,
    fetchTodos,
    addTodo,
    updateTodo,
    deleteTodo,
  }
})
