import { vi } from 'vitest'
import { config } from '@vue/test-utils'
import ElementPlus from 'element-plus'

// Mock Element Plus icons
vi.mock('@element-plus/icons-vue', () => ({
  Plus: {
    name: 'Plus',
    template: '<span class="el-icon-plus">+</span>'
  },
  Edit: {
    name: 'Edit',
    template: '<span class="el-icon-edit">✏️</span>'
  },
  Delete: {
    name: 'Delete',
    template: '<span class="el-icon-delete">🗑️</span>'
  },
  Check: {
    name: 'Check',
    template: '<span class="el-icon-check">✓</span>'
  }
}))

// Mock dayjs
vi.mock('dayjs', () => {
  const dayjs = (date?: any) => ({
    format: (format: string) => {
      if (date) {
        const d = new Date(date)
        return `${d.getMonth() + 1}/${d.getDate()}/${d.getFullYear()} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`
      }
      return '01/01/2024 00:00'
    },
    startOf: (unit: string) => ({
      valueOf: () => new Date().setHours(0, 0, 0, 0)
    }),
    valueOf: () => new Date().valueOf()
  })

  dayjs.extend = () => {}
  return { default: dayjs }
})

// Global test configuration
config.global.plugins = [ElementPlus]

// Mock console methods to reduce noise in tests
global.console = {
  ...console,
  warn: vi.fn(),
  error: vi.fn(),
}
