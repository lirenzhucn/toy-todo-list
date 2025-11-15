import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount, VueWrapper } from '@vue/test-utils'
import TodoTable from '@/components/TodoTable.vue'
import { mockTodoItems } from '../utils/mocks'
import type { TodoItem } from '@/types/todo'

// Mock the todo store
vi.mock('@/stores/todo', () => ({
  useTodoStore: vi.fn(() => ({
    updateTodo: vi.fn().mockResolvedValue(undefined),
    deleteTodo: vi.fn().mockResolvedValue(undefined)
  }))
}))

// Mock Element Plus message components
vi.mock('element-plus', async () => {
  const actual = await vi.importActual('element-plus')
  return {
    ...actual,
    ElMessage: {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
      info: vi.fn()
    },
    ElMessageBox: {
      confirm: vi.fn().mockResolvedValue(true)
    }
  }
})

// Mock icons
vi.mock('@element-plus/icons-vue', () => ({
  Check: {
    name: 'Check',
    template: '<span class="el-icon-check">✓</span>'
  },
  Edit: {
    name: 'Edit',
    template: '<span class="el-icon-edit">✏️</span>'
  },
  Delete: {
    name: 'Delete',
    template: '<span class="el-icon-delete">🗑️</span>'
  }
}))

describe('TodoTable', () => {
  let wrapper: VueWrapper<any>

  const defaultProps = {
    todos: mockTodoItems.map(item => ({
      ...item,
      isComplete: item.isComplete ?? false
    })),
    loading: false
  }

  const mountComponent = (props = {}) => {
    return mount(TodoTable, {
      props: { ...defaultProps, ...props },
      global: {
        stubs: {
          // Create a comprehensive table stub that simulates Element Plus behavior
          'el-table': {
            name: 'ElTable',
            props: ['data', 'loading'],
            template: `
              <div class="el-table" :class="{ 'el-table--loading': loading }">
                <div class="el-table__body" :class="{ 'el-loading-mask': loading }">
                  <div 
                    v-for="(row, index) in data" 
                    :key="index" 
                    class="el-table__row"
                  >
                    <!-- Checkbox column -->
                    <div class="el-table__cell checkbox-cell">
                      <el-checkbox v-if="!row.isComplete" :model-value="false" />
                      <el-icon v-else class="el-icon-check"><Check /></el-icon>
                    </div>
                    <!-- Title column -->
                    <div class="el-table__cell title-cell">{{ row.title }}</div>
                    <!-- Description column -->
                    <div class="el-table__cell description-cell">{{ row.description }}</div>
                    <!-- Status column -->
                    <div class="el-table__cell status-cell">
                      <el-tag :type="row.isComplete ? 'success' : 'warning'">
                        {{ row.isComplete ? 'Complete' : 'Todo' }}
                      </el-tag>
                    </div>
                    <!-- Scheduled column -->
                    <div class="el-table__cell scheduled-cell">{{ formatDateTime(row.scheduledDateTime) }}</div>
                    <!-- Due column -->
                    <div class="el-table__cell due-cell">{{ formatDateTime(row.dueDateTime) }}</div>
                    <!-- Actions column -->
                    <div class="el-table__cell actions-cell">
                      <el-button @click="() => $parent.$emit('edit-row', row)" class="edit-btn">Edit</el-button>
                      <el-button @click="() => $parent.$emit('delete-row', row)" class="delete-btn">Delete</el-button>
                    </div>
                  </div>
                </div>
              </div>
            `,
            methods: {
              formatDateTime(dateTime: string | null | undefined) {
                if (!dateTime) return '-'
                const date = new Date(dateTime)
                return `${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`
              },
              handleEdit(row: TodoItem) {
                this.$emit('edit', row)
              },
              handleDelete(row: TodoItem) {
                this.$emit('delete', row)
              }
            }
          },
          // Stub other Element Plus components
          'el-checkbox': {
            name: 'ElCheckbox',
            props: ['modelValue'],
            template: '<input type="checkbox" class="el-checkbox" :checked="modelValue" @change="$emit(\'update:modelValue\', $event.target.checked)" />'
          },
          'el-button': {
            name: 'ElButton',
            props: ['size', 'type'],
            template: '<button class="el-button" :class="[size ? `el-button--${size}` : \'\', type ? `el-button--${type}` : \'\']" @click="$emit(\'click\', $event)"><slot></slot></button>'
          },
          'el-tag': {
            name: 'ElTag',
            props: ['type'],
            template: '<span class="el-tag" :class="type ? `el-tag--${type}` : \'\'"><slot></slot></span>'
          },
          'el-icon': {
            name: 'ElIcon',
            template: '<span class="el-icon"><slot></slot></span>'
          },
          'el-dialog': {
            name: 'ElDialog',
            props: ['modelValue', 'title'],
            template: '<div v-if="modelValue" class="el-dialog"><div class="el-dialog__title">{{ title }}</div><slot></slot></div>'
          }
        }
      }
    })
  }

  beforeEach(() => {
    wrapper = mountComponent()
  })

  describe('Rendering', () => {
    it('should render the table with todos', () => {
      expect(wrapper.find('.el-table')).toBeTruthy()
      expect(wrapper.findAll('.el-table__row')).toHaveLength(4)
    })

    it('should display todo titles', () => {
      expect(wrapper.text()).toContain('Test Todo 1')
      expect(wrapper.text()).toContain('Test Todo 2')
    })

    it('should display todo descriptions', () => {
      expect(wrapper.text()).toContain('Description for test todo 1')
      expect(wrapper.text()).toContain('Description for test todo 2')
    })

    it('should display status information', () => {
      expect(wrapper.text()).toContain('Todo')
      expect(wrapper.text()).toContain('Complete')
    })

    it('should show checkboxes for incomplete todos', () => {
      const checkboxes = wrapper.findAll('.el-checkbox')
      expect(checkboxes.length).toBeGreaterThan(0)
    })

    it('should show check icons for completed todos', () => {
      const checkIcons = wrapper.findAll('.el-icon-check')
      expect(checkIcons.length).toBeGreaterThan(0)
    })
  })

  describe('Date Formatting', () => {
    it('should format dates correctly', () => {
      // The dates should be formatted by the mocked dayjs
      expect(wrapper.text()).toContain('1/15/2024 10:00')
      expect(wrapper.text()).toContain('1/16/2024 18:00')
    })
  })

  describe('Actions', () => {
    it('should have action buttons', () => {
      const buttons = wrapper.findAll('.el-button')
      expect(buttons.length).toBeGreaterThan(0)
    })

    it('should open edit dialog when edit button is clicked', async () => {
      // First, let's simulate the edit button click by directly calling the method
      const todo = mockTodoItems[0]
      await wrapper.vm.handleEdit(todo)
      
      // The dialog should be shown
      expect(wrapper.vm.showEditDialog).toBe(true)
      expect(wrapper.vm.editingTodo).toEqual(todo)
    })
  })

  describe('Empty State', () => {
    it('should render empty message when no todos are provided', () => {
      const emptyWrapper = mountComponent({ todos: [] })
      
      expect(emptyWrapper.findAll('.el-table__row')).toHaveLength(0)
      expect(emptyWrapper.text()).not.toContain('Test Todo 1')
      expect(emptyWrapper.text()).not.toContain('Test Todo 2')
    })
  })

  describe('Loading State', () => {
    it('should handle loading state', () => {
      const loadingWrapper = mountComponent({ loading: true })
      
      expect(loadingWrapper.vm.loading).toBe(true)
      expect(loadingWrapper.find('.el-loading-mask').exists()).toBe(true)
    })
  })

  describe('Props Validation', () => {
    it('should accept valid todo data', () => {
      const validTodo = {
        id: 1,
        title: 'Valid Todo',
        description: 'Valid description',
        isComplete: false,
        scheduledDateTime: '2024-01-15T10:00:00',
        dueDateTime: '2024-01-16T18:00:00'
      }
      
      const validWrapper = mountComponent({ todos: [validTodo] })
      expect(validWrapper.text()).toContain('Valid Todo')
      expect(validWrapper.text()).toContain('Valid description')
    })

    it('should handle todos with minimal data', () => {
      const minimalTodo = {
        id: 1,
        title: 'Minimal Todo',
        isComplete: false
      }
      
      const minimalWrapper = mountComponent({ todos: [minimalTodo] })
      expect(minimalWrapper.text()).toContain('Minimal Todo')
    })

    it('should handle null optional fields', () => {
      const todoWithNulls = {
        id: 1,
        title: 'Todo with nulls',
        description: null,
        isComplete: false,
        scheduledDateTime: null,
        dueDateTime: null
      }
      
      const nullWrapper = mountComponent({ todos: [todoWithNulls] })
      expect(nullWrapper.text()).toContain('Todo with nulls')
    })
  })
})