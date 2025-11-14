import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { TodoItem } from '@/types/todo'
import { TodoApiClient } from '@/api'
import dayjs from 'dayjs'

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
  const incompleteTodos = computed(() => {
    const incomplete = todos.value.filter(todo => !todo.isComplete)
    return sortTodosByDate(incomplete)
  })
  const overdueTodos = computed(() => {
    const now = new Date()
    return todos.value.filter(todo =>
      todo.dueDateTime && new Date(todo.dueDateTime) < now && !todo.isComplete
    )
  })
  const todayTodos = computed(() => {
    const today = dayjs().startOf('day')
    const todayItems = todos.value.filter(todo => {
      if (todo.isComplete) return false

      const scheduledDate = todo.scheduledDateTime ? dayjs(todo.scheduledDateTime) : null
      const dueDate = todo.dueDateTime ? dayjs(todo.dueDateTime) : null

      // Include if scheduled today or before today
      const scheduledTodayOrBefore = scheduledDate && scheduledDate <= today
      // Include if due today or before today
      const dueTodayOrBefore = dueDate && dueDate <= today

      return scheduledTodayOrBefore || dueTodayOrBefore
    })
    return sortTodosByDate(todayItems)
  })

  // Helper function to sort todos by scheduled date, then due date
  function sortTodosByDate(todos: TodoItem[]): TodoItem[] {
    return [...todos].sort((a, b) => {
      // First sort by scheduled date
      const aScheduled = a.scheduledDateTime ? dayjs(a.scheduledDateTime) : null
      const bScheduled = b.scheduledDateTime ? dayjs(b.scheduledDateTime) : null

      if (aScheduled && bScheduled) {
        const scheduledDiff = aScheduled.valueOf() - bScheduled.valueOf()
        if (scheduledDiff !== 0) return scheduledDiff
      } else if (aScheduled && !bScheduled) {
        return -1 // Items with scheduled dates come first
      } else if (!aScheduled && bScheduled) {
        return 1 // Items with scheduled dates come first
      }

      // Then sort by due date
      const aDue = a.dueDateTime ? dayjs(a.dueDateTime) : null
      const bDue = b.dueDateTime ? dayjs(b.dueDateTime) : null

      if (aDue && bDue) {
        return aDue.valueOf() - bDue.valueOf()
      } else if (aDue && !bDue) {
        return -1 // Items with due dates come first
      } else if (!aDue && bDue) {
        return 1 // Items with due dates come first
      }

      return 0
    })
  }

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
    todayTodos,
    fetchTodos,
    addTodo,
    updateTodo,
    deleteTodo,
  }
})
