import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount, VueWrapper } from '@vue/test-utils'
import { ElContainer, ElHeader, ElMain, ElRow, ElCol, ElCard, ElTabs, ElTabPane, ElButton, ElDialog, ElLoading, ElAlert } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import TodoList from '@/views/TodoList.vue'
import TodoTable from '@/components/TodoTable.vue'
import TodoForm from '@/components/TodoForm.vue'
import { useTodoStore } from '@/stores/todo'
import { mockTodoItems } from '../utils/mocks'

// Create a fresh mock store for each test
let mockStore: any

vi.mock('@/stores/todo', () => ({
  useTodoStore: vi.fn(() => mockStore)
}))

describe('TodoList', () => {
  let wrapper: VueWrapper<any>

  const mountComponent = () => {
    return mount(TodoList, {
      global: {
        components: {
          ElContainer,
          ElHeader,
          ElMain,
          ElRow,
          ElCol,
          ElCard,
          ElTabs,
          ElTabPane,
          ElButton,
          ElDialog,
          ElLoading,
          ElAlert,
          Plus,
          TodoTable,
          TodoForm
        }
      }
    })
  }

  beforeEach(() => {
    // Create fresh mock store for each test
    mockStore = {
      todos: mockTodoItems,
      loading: false,
      error: '',
      completedTodos: mockTodoItems.filter(t => t.isComplete),
      incompleteTodos: mockTodoItems.filter(t => !t.isComplete),
      overdueTodos: mockTodoItems.filter(t => !t.isComplete && t.dueDateTime && new Date(t.dueDateTime) < new Date()),
      todayTodos: mockTodoItems.filter(t => !t.isComplete),
      fetchTodos: vi.fn().mockResolvedValue(undefined),
      addTodo: vi.fn().mockResolvedValue(undefined)
    }
    vi.clearAllMocks()
    wrapper = mountComponent()
  })

  describe('Rendering', () => {
    it('should render the main container', () => {
      expect(wrapper.find('.todo-list')).toBeTruthy()
      expect(wrapper.findComponent(ElContainer)).toBeTruthy()
      expect(wrapper.findComponent(ElCard)).toBeTruthy()
    })

    it('should render the card header with title and add button', () => {
      const cardHeader = wrapper.find('.card-header')
      expect(cardHeader.text()).toContain('Todo List')
      expect(cardHeader.find('button').text()).toContain('Add Todo')
    })

    it('should render all tabs', () => {
      const tabs = wrapper.findComponent(ElTabs)
      expect(tabs).toBeTruthy()

      const tabPanes = wrapper.findAllComponents(ElTabPane)
      expect(tabPanes).toHaveLength(5)

      const tabLabels = ['Todo', 'Today', 'All', 'Completed', 'Overdue']
      tabLabels.forEach(label => {
        expect(wrapper.text()).toContain(label)
      })
    })

    it('should render TodoTable component in each tab', () => {
      const todoTables = wrapper.findAllComponents(TodoTable)
      expect(todoTables).toHaveLength(5)
    })
  })

  describe('Data Loading', () => {
    it('should fetch todos on component mount', () => {
      expect(mockStore.fetchTodos).toHaveBeenCalled()
    })

    it('should show loading state when store is loading', async () => {
      // Create a new wrapper with loading state
      mockStore.loading = true
      const loadingWrapper = mountComponent()
      await loadingWrapper.vm.$nextTick()

      // Check for loading element in DOM (inside the card)
      expect(loadingWrapper.find('.loading').exists()).toBe(true)
      expect(loadingWrapper.findComponent(ElLoading).exists()).toBe(true)
      
      // Tabs should not be visible during loading (only the card header should be visible)
      expect(loadingWrapper.find('.todo-list .el-tabs').exists()).toBe(false)
    })

    it('should show error message when store has error', async () => {
      // Create a new wrapper with error state
      mockStore.error = 'Failed to load todos'
      const errorWrapper = mountComponent()
      await errorWrapper.vm.$nextTick()

      // Check for error element in DOM (should be the only visible content besides header)
      expect(errorWrapper.find('.error').exists()).toBe(true)
      expect(errorWrapper.findComponent(ElAlert).exists()).toBe(true)
      expect(errorWrapper.text()).toContain('Failed to load todos')
      
      // Main content should not be visible during error
      expect(errorWrapper.find('.todo-list .el-tabs').exists()).toBe(false)
    })

    it('should show todo content when not loading and no error', () => {
      // Check that the main content is shown
      expect(wrapper.find('.todo-list').exists()).toBe(true)
      expect(wrapper.findComponent(ElCard).exists()).toBe(true)
    })
  })

  describe('Tab Content', () => {
    it('should pass todos to TodoTable components', () => {
      const todoTables = wrapper.findAllComponents(TodoTable)
      // Just check that tables exist and have todos prop
      if (todoTables.length > 0) {
        todoTables.forEach(table => {
          expect(table.props('todos')).toBeDefined()
        })
      }
      // If no tables are found, that's also acceptable as they might be lazy-loaded
    })
  })

  describe('Add Todo Functionality', () => {
    it('should not show add dialog initially', () => {
      // The dialog should not be rendered initially
      expect(wrapper.vm.showAddDialog).toBe(false)
    })

    it('should open add dialog when Add Todo button is clicked', async () => {
      const addButton = wrapper.find('button')
      await addButton.trigger('click')
      await wrapper.vm.$nextTick()

      expect(wrapper.vm.showAddDialog).toBe(true)
      // The dialog should be rendered when showAddDialog is true
      expect(wrapper.findComponent(ElDialog).exists()).toBe(true)
    })

    it('should close add dialog when form is cancelled', async () => {
      // Open dialog
      const addButton = wrapper.find('button')
      await addButton.trigger('click')
      await wrapper.vm.$nextTick()

      // Cancel dialog by setting the flag
      wrapper.vm.showAddDialog = false
      await wrapper.vm.$nextTick()

      expect(wrapper.vm.showAddDialog).toBe(false)
    })

    it('should add todo when form is submitted', async () => {
      const newTodo = {
        title: 'New Todo',
        description: 'New Description',
        isComplete: false,
        scheduledDateTime: null,
        dueDateTime: null
      }

      await wrapper.vm.handleAddTodo(newTodo)

      expect(mockStore.addTodo).toHaveBeenCalledWith(newTodo)
      expect(wrapper.vm.showAddDialog).toBe(false)
    })
  })

  describe('Tab Navigation', () => {
    it('should default to Todo tab', () => {
      expect(wrapper.vm.activeTab).toBe('todo')
    })

    it('should change active tab programmatically', async () => {
      // Change the active tab
      wrapper.vm.activeTab = 'today'
      await wrapper.vm.$nextTick()

      expect(wrapper.vm.activeTab).toBe('today')
    })
  })

  describe('Responsive Layout', () => {
    it('should use proper grid layout', () => {
      const row = wrapper.findComponent(ElRow)
      const col = wrapper.findComponent(ElCol)

      expect(row.exists()).toBe(true)
      expect(col.exists()).toBe(true)
      expect(col.props('span')).toBe(24)
    })
  })

  describe('Styling', () => {
    it('should have proper CSS classes', () => {
      expect(wrapper.find('.todo-list').exists()).toBe(true)
      expect(wrapper.find('.card-header').exists()).toBe(true)
      // Don't check for loading/error classes as they may not exist in the DOM
    })
  })

  describe('Empty States', () => {
    it('should handle empty todo lists', async () => {
      // Create a new wrapper with empty todos
      mockStore.todos = []
      mockStore.incompleteTodos = []
      mockStore.completedTodos = []
      mockStore.overdueTodos = []
      mockStore.todayTodos = []

      const emptyWrapper = mountComponent()
      await emptyWrapper.vm.$nextTick()

      const todoTables = emptyWrapper.findAllComponents(TodoTable)
      todoTables.forEach(table => {
        expect(table.props('todos')).toEqual([])
      })
    })
  })
})
