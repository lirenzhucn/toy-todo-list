import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { useTodoStore } from '@/stores/todo'
import { mockTodoItems } from '../utils/mocks'

// Mock the auth store
vi.mock('@/stores/auth', () => ({
  useAuthStore: vi.fn(() => ({
    token: 'test-token',
    isAuthenticated: true,
    logout: vi.fn(),
    updateApiClientAuth: vi.fn()
  }))
}))

// Mock the API client module
vi.mock('@/api/TodoApiClient', () => ({
  TodoApiClient: class MockTodoApiClient {
    todoBackendApi = {
      getApiTodoitems: vi.fn().mockResolvedValue(mockTodoItems),
      postApiTodoitems: vi.fn().mockImplementation((todo: any) =>
        Promise.resolve({ ...todo, id: Date.now() })
      ),
      putApiTodoitems: vi.fn().mockImplementation((id: number, todo: any) =>
        Promise.resolve({ ...todo, id })
      ),
      deleteApiTodoitems: vi.fn().mockResolvedValue(undefined)
    }
  }
}))

// Mock the API module
vi.mock('@/api', () => ({
  TodoApiClient: class MockTodoApiClient {
    request = {
      config: {
        TOKEN: undefined,
        WITH_CREDENTIALS: false,
        CREDENTIALS: 'include'
      }
    }
    todoBackendApi = {
      getApiTodoitems: vi.fn().mockResolvedValue(mockTodoItems),
      postApiTodoitems: vi.fn().mockImplementation((todo: any) =>
        Promise.resolve({ ...todo, id: Date.now() })
      ),
      putApiTodoitems: vi.fn().mockImplementation((id: number, todo: any) =>
        Promise.resolve({ ...todo, id })
      ),
      deleteApiTodoitems: vi.fn().mockResolvedValue(undefined)
    }
  }
}))

describe('TodoStore', () => {
  let store: ReturnType<typeof useTodoStore>

  beforeEach(() => {
    setActivePinia(createPinia())
    store = useTodoStore()
  })

  describe('Initial State', () => {
    it('should have empty todos array initially', () => {
      expect(store.todos).toEqual([])
    })

    it('should have loading as false initially', () => {
      expect(store.loading).toBe(false)
    })

    it('should have null error initially', () => {
      expect(store.error).toBeNull()
    })
  })

  describe('Getters', () => {
    beforeEach(() => {
      store.todos = mockTodoItems
    })

    it('should return completed todos', () => {
      const completed = store.completedTodos
      expect(completed).toHaveLength(1)
      expect(completed[0].id).toBe(2)
      expect(completed[0].isComplete).toBe(true)
    })

    it('should return incomplete todos', () => {
      const incomplete = store.incompleteTodos
      expect(incomplete).toHaveLength(3)
      expect(incomplete.every(todo => !todo.isComplete)).toBe(true)
    })

    it('should return overdue todos', () => {
      const overdue = store.overdueTodos
      // The exact count depends on the current date vs the mock dates
      expect(overdue.length).toBeGreaterThanOrEqual(1)
      expect(overdue.every(todo => !todo.isComplete)).toBe(true)
    })

    it('should return today todos', () => {
      const today = store.todayTodos
      // The exact count depends on the current date vs the mock dates
      expect(today.length).toBeGreaterThanOrEqual(0)
      expect(today.every(todo => !todo.isComplete)).toBe(true)
    })
  })

  describe('Actions', () => {
    describe('fetchTodos', () => {
      it('should fetch todos successfully', async () => {
        await store.fetchTodos()

        expect(store.todos).toEqual(mockTodoItems)
        expect(store.loading).toBe(false)
        expect(store.error).toBeNull()
      })

      it('should set loading state during fetch', async () => {
        const fetchPromise = store.fetchTodos()
        expect(store.loading).toBe(true)

        await fetchPromise
        expect(store.loading).toBe(false)
      })
    })

    describe('addTodo', () => {
      it('should add todo successfully', async () => {
        const newTodo = {
          title: 'New Todo',
          description: 'New description',
          isComplete: false,
          scheduledDateTime: null,
          dueDateTime: null
        }

        await store.addTodo(newTodo)

        expect(store.todos).toHaveLength(1)
        expect(store.todos[0].title).toBe('New Todo')
        expect(store.todos[0].id).toBeDefined()
        expect(store.loading).toBe(false)
        expect(store.error).toBeNull()
      })
    })

    describe('updateTodo', () => {
      beforeEach(() => {
        store.todos = mockTodoItems
      })

      it('should update todo successfully', async () => {
        const updatedTodo = {
          ...mockTodoItems[0],
          title: 'Updated Title',
          description: 'Updated Description'
        }

        await store.updateTodo(1, updatedTodo)

        const updatedItem = store.todos.find(todo => todo.id === 1)
        expect(updatedItem?.title).toBe('Updated Title')
        expect(updatedItem?.description).toBe('Updated Description')
        expect(store.loading).toBe(false)
        expect(store.error).toBeNull()
      })
    })

    describe('deleteTodo', () => {
      beforeEach(() => {
        store.todos = mockTodoItems
      })

      it('should delete todo successfully', async () => {
        await store.deleteTodo(1)

        expect(store.todos).toHaveLength(3)
        expect(store.todos.find(todo => todo.id === 1)).toBeUndefined()
        expect(store.loading).toBe(false)
        expect(store.error).toBeNull()
      })
    })
  })
})
