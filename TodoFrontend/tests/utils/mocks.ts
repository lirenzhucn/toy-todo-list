import { vi } from 'vitest'
import type { TodoItem } from '@/types/todo'

export const mockTodoItems: TodoItem[] = [
  {
    id: 1,
    title: 'Test Todo 1',
    description: 'Description for test todo 1',
    isComplete: false,
    scheduledDateTime: '2024-01-15T10:00:00',
    dueDateTime: '2024-01-16T18:00:00'
  },
  {
    id: 2,
    title: 'Test Todo 2',
    description: 'Description for test todo 2',
    isComplete: true,
    scheduledDateTime: '2024-01-14T09:00:00',
    dueDateTime: '2024-01-15T17:00:00'
  },
  {
    id: 3,
    title: 'Overdue Todo',
    description: 'This todo is overdue',
    isComplete: false,
    scheduledDateTime: '2024-01-10T08:00:00',
    dueDateTime: '2024-01-11T16:00:00'
  },
  {
    id: 4,
    title: 'Today Todo',
    description: 'Todo scheduled for today',
    isComplete: false,
    scheduledDateTime: new Date().toISOString(),
    dueDateTime: null
  }
]

export const createMockApiClient = () => ({
  todoBackendApi: {
    getApiTodoitems: vi.fn().mockResolvedValue(mockTodoItems),
    postApiTodoitems: vi.fn().mockImplementation((todo: Omit<TodoItem, 'id'>) =>
      Promise.resolve({ ...todo, id: Date.now() })
    ),
    putApiTodoitems: vi.fn().mockImplementation((id: number, todo: TodoItem) =>
      Promise.resolve({ ...todo, id })
    ),
    deleteApiTodoitems: vi.fn().mockResolvedValue(undefined)
  }
})

export const mockElMessage = {
  success: vi.fn(),
  error: vi.fn(),
  warning: vi.fn(),
  info: vi.fn()
}

export const mockElMessageBox = {
  confirm: vi.fn().mockResolvedValue(true)
}
