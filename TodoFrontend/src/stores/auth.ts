import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { LoginRequest } from '@/api/models/LoginRequest'
import type { RegisterRequest } from '@/api/models/RegisterRequest'
import { TodoApiClient } from '@/api'
import { useTodoStore } from './todo'

// Define AuthResponse type locally since it's not in the auto-generated API models
interface AuthResponse {
  token: string;
  userName: string;
  email: string;
  expiration: string;
}

export const useAuthStore = defineStore('auth', () => {
  const apiClient = new TodoApiClient({
    // this trick uses the dynamic vite api url to point the client object to the correct backend url
    BASE: import.meta.env.VITE_API_BASE_URL || '',
  })

  function updateApiClients() {
    // Update the todo store's API client with the current authentication state
    const todoStore = useTodoStore()
    todoStore.updateApiClientAuth()
  }
  // State
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<{ userName: string; email: string } | null>(
    localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')!) : null
  )
  const expiration = ref<string | null>(localStorage.getItem('expiration'))

  // Getters
  const isAuthenticated = computed(() => {
    if (!token.value || !expiration.value) return false

    // Check if token is expired
    const expirationDate = new Date(expiration.value)
    const now = new Date()
    return expirationDate > now
  })

  const userName = computed(() => user.value?.userName || '')
  const userEmail = computed(() => user.value?.email || '')

  // Actions
  function setAuthData(authResponse: AuthResponse) {
    token.value = authResponse.token
    user.value = {
      userName: authResponse.userName,
      email: authResponse.email
    }
    expiration.value = authResponse.expiration

    // Persist to localStorage
    localStorage.setItem('token', authResponse.token)
    localStorage.setItem('user', JSON.stringify(user.value))
    localStorage.setItem('expiration', authResponse.expiration)

    // Update any existing API clients with the new token
    updateApiClients()
  }

  function clearAuthData() {
    token.value = null
    user.value = null
    expiration.value = null

    // Clear from localStorage
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('expiration')

    // Update any existing API clients to remove the token
    updateApiClients()
  }

  async function login(loginRequest: LoginRequest): Promise<void> {
    try {
      const response = await apiClient.todoBackendApi.postApiAuthLogin(loginRequest)
      const authData = response as AuthResponse
      setAuthData(authData)
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    }
  }

  async function register(registerRequest: RegisterRequest): Promise<void> {
    try {
      const response = await apiClient.todoBackendApi.postApiAuthRegister(registerRequest)
      // Registration successful - you can choose to auto-login or require manual login
      // For now, we'll just return success and let the user login manually
    } catch (error) {
      console.error('Registration failed:', error)
      throw error
    }
  }

  function logout() {
    clearAuthData()
  }

  // Initialize store from localStorage on creation
  function initializeAuth() {
    const storedToken = localStorage.getItem('token')
    const storedUser = localStorage.getItem('user')
    const storedExpiration = localStorage.getItem('expiration')

    if (storedToken && storedUser && storedExpiration) {
      token.value = storedToken
      user.value = JSON.parse(storedUser)
      expiration.value = storedExpiration
    }
  }

  return {
    // State
    token,
    user,
    expiration,

    // Getters
    isAuthenticated,
    userName,
    userEmail,

    // Actions
    login,
    register,
    logout,
    initializeAuth,
    setAuthData,
    clearAuthData,
    updateApiClients
  }
})


