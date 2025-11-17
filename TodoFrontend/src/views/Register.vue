<template>
  <div class="register-container">
    <el-card class="register-card">
      <template #header>
        <div class="card-header">
          <span>Register</span>
        </div>
      </template>

      <el-form
        ref="registerFormRef"
        :model="registerForm"
        :rules="registerRules"
        label-width="80px"
        @submit.prevent="handleRegister"
      >
        <el-form-item label="Email" prop="email">
          <el-input
            v-model="registerForm.email"
            type="email"
            placeholder="Enter your email"
            prefix-icon="Message"
          />
        </el-form-item>

        <el-form-item label="Username" prop="userName">
          <el-input
            v-model="registerForm.userName"
            placeholder="Enter your username"
            prefix-icon="User"
          />
        </el-form-item>

        <el-form-item label="Password" prop="password">
          <el-input
            v-model="registerForm.password"
            type="password"
            placeholder="Enter your password"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>

        <el-form-item label="Confirm" prop="confirmPassword">
          <el-input
            v-model="registerForm.confirmPassword"
            type="password"
            placeholder="Confirm your password"
            prefix-icon="Lock"
            show-password
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            :loading="loading"
            @click="handleRegister"
            style="width: 100%"
          >
            Register
          </el-button>
        </el-form-item>

        <el-form-item>
          <div class="form-footer">
            <span>Already have an account?</span>
            <el-link type="primary" @click="goToLogin">Login here</el-link>
          </div>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { useAuthStore } from '@/stores/auth'
import type { RegisterRequest } from '@/api/models/RegisterRequest'

const router = useRouter()
const authStore = useAuthStore()

const registerFormRef = ref<FormInstance>()
const loading = ref(false)

interface RegisterForm extends RegisterRequest {
  confirmPassword: string
}

const registerForm = reactive<RegisterForm>({
  email: '',
  userName: '',
  password: '',
  confirmPassword: ''
})

const validateConfirmPassword = (rule: any, value: string, callback: any) => {
  if (value === '') {
    callback(new Error('Please confirm your password'))
  } else if (value !== registerForm.password) {
    callback(new Error('Passwords do not match!'))
  } else {
    callback()
  }
}

const registerRules: FormRules = {
  email: [
    { required: true, message: 'Please enter your email', trigger: 'blur' },
    { type: 'email', message: 'Please enter a valid email', trigger: 'blur' }
  ],
  userName: [
    { required: true, message: 'Please enter your username', trigger: 'blur' },
    { min: 3, message: 'Username must be at least 3 characters', trigger: 'blur' }
  ],
  password: [
    { required: true, message: 'Please enter your password', trigger: 'blur' },
    { min: 6, message: 'Password must be at least 6 characters', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, validator: validateConfirmPassword, trigger: 'blur' }
  ]
}

const handleRegister = async () => {
  if (!registerFormRef.value) return

  try {
    await registerFormRef.value.validate()
    loading.value = true

    const { confirmPassword, ...registerData } = registerForm
    await authStore.register(registerData)

    ElMessage.success('Registration successful! Please login.')
    router.push('/login')
  } catch (error: any) {
    console.error('Registration error:', error)
    ElMessage.error(error.response?.data?.message || 'Registration failed. Please try again.')
  } finally {
    loading.value = false
  }
}

const goToLogin = () => {
  router.push('/login')
}
</script>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: calc(100vh - 120px);
  padding: 20px;
}

.register-card {
  width: 100%;
  max-width: 400px;
}

.card-header {
  text-align: center;
  font-size: 24px;
  font-weight: bold;
}

.form-footer {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  width: 100%;
}
</style>
