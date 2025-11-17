<template>
  <div id="app">
    <el-container>
      <el-header>
        <div class="header-content">
          <h1>Todo List Management</h1>
          <div class="auth-section">
            <template v-if="authStore.isAuthenticated">
              <span class="welcome-text">Welcome, {{ authStore.userName }}</span>
              <el-button type="danger" size="small" @click="handleLogout">
                Logout
              </el-button>
            </template>
            <template v-else>
              <el-button type="primary" size="small" @click="goToLogin">
                Login
              </el-button>
              <el-button type="success" size="small" @click="goToRegister">
                Register
              </el-button>
            </template>
          </div>
        </div>
      </el-header>
      <el-main>
        <router-view />
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { RouterView, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'

const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  ElMessage.success('Logged out successfully')
  router.push('/login')
}

const goToLogin = () => {
  router.push('/login')
}

const goToRegister = () => {
  router.push('/register')
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}

.el-header {
  background-color: #409eff;
  color: white;
  line-height: 60px;
  padding: 0 20px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  max-width: 1200px;
  margin: 0 auto;
}

.header-content h1 {
  margin: 0;
  font-size: 24px;
}

.auth-section {
  display: flex;
  align-items: center;
  gap: 10px;
}

.welcome-text {
  margin-right: 10px;
  font-size: 14px;
}
</style>