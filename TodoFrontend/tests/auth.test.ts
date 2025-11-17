import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'

describe('Authentication System', () => {
  beforeEach(() => {
    // Create a fresh Pinia instance for each test
    setActivePinia(createPinia())

    // Clear localStorage
    localStorage.clear()
  })

  it('should initialize with no authentication', () => {
    const authStore = useAuthStore()

    expect(authStore.isAuthenticated).toBe(false)
    expect(authStore.token).toBeNull()
    expect(authStore.user).toBeNull()
    expect(authStore.userName).toBe('')
    expect(authStore.userEmail).toBe('')
  })

  it('should set authentication data correctly', () => {
    const authStore = useAuthStore()

    const mockAuthResponse = {
      token: 'test-token-123',
      expiration: new Date(Date.now() + 3600000).toISOString(), // 1 hour from now
      userName: 'testuser',
      email: 'test@example.com'
    }

    authStore.setAuthData(mockAuthResponse)

    expect(authStore.isAuthenticated).toBe(true)
    expect(authStore.token).toBe('test-token-123')
    expect(authStore.userName).toBe('testuser')
    expect(authStore.userEmail).toBe('test@example.com')

    // Check localStorage
    expect(localStorage.getItem('token')).toBe('test-token-123')
    expect(localStorage.getItem('user')).toBe('{"userName":"testuser","email":"test@example.com"}')
    expect(localStorage.getItem('expiration')).toBe(mockAuthResponse.expiration)
  })

  it('should clear authentication data correctly', () => {
    const authStore = useAuthStore()

    // First set some auth data
    const mockAuthResponse = {
      token: 'test-token-123',
      expiration: new Date(Date.now() + 3600000).toISOString(),
      userName: 'testuser',
      email: 'test@example.com'
    }

    authStore.setAuthData(mockAuthResponse)

    // Then clear it
    authStore.clearAuthData()

    expect(authStore.isAuthenticated).toBe(false)
    expect(authStore.token).toBeNull()
    expect(authStore.user).toBeNull()
    expect(authStore.userName).toBe('')
    expect(authStore.userEmail).toBe('')

    // Check localStorage is cleared
    expect(localStorage.getItem('token')).toBeNull()
    expect(localStorage.getItem('user')).toBeNull()
    expect(localStorage.getItem('expiration')).toBeNull()
  })

  it('should detect expired tokens', () => {
    const authStore = useAuthStore()

    const expiredAuthResponse = {
      token: 'expired-token-123',
      expiration: new Date(Date.now() - 3600000).toISOString(), // 1 hour ago
      userName: 'testuser',
      email: 'test@example.com'
    }

    authStore.setAuthData(expiredAuthResponse)

    expect(authStore.isAuthenticated).toBe(false)
  })

  it('should initialize from localStorage', () => {
    // Set up localStorage with auth data
    const mockAuthResponse = {
      token: 'stored-token-123',
      expiration: new Date(Date.now() + 3600000).toISOString(),
      userName: 'storeduser',
      email: 'stored@example.com'
    }

    localStorage.setItem('token', mockAuthResponse.token)
    localStorage.setItem('user', JSON.stringify({ userName: mockAuthResponse.userName, email: mockAuthResponse.email }))
    localStorage.setItem('expiration', mockAuthResponse.expiration)

    // Create a new store instance
    const authStore = useAuthStore()
    authStore.initializeAuth()

    expect(authStore.isAuthenticated).toBe(true)
    expect(authStore.token).toBe('stored-token-123')
    expect(authStore.userName).toBe('storeduser')
    expect(authStore.userEmail).toBe('stored@example.com')
  })
})
