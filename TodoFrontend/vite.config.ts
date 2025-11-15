import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5161',
        changeOrigin: true,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./tests/setup.ts'],
    coverage: {
      provider: 'v8',
      exclude: [
        'tests/**',
        'src/api/**',
        'coverage/**',
        'dist/**',
        'node_modules/**',
        '**/*.config.*',
        '**/*.d.ts',
        '**/*.test.*',
        '**/*.spec.*',
        '**/__tests__/**'
      ]
    }
  }
})