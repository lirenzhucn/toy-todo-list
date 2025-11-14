<template>
  <el-form
    ref="formRef"
    :model="formData"
    :rules="rules"
    label-width="120px"
  >
    <el-form-item label="Title" prop="title">
      <el-input v-model="formData.title" placeholder="Enter todo title" />
    </el-form-item>

    <el-form-item label="Description" prop="description">
      <el-input
        v-model="formData.description"
        type="textarea"
        placeholder="Enter description (optional)"
        :rows="3"
      />
    </el-form-item>

    <el-form-item label="Status" prop="isComplete">
      <el-switch
        v-model="formData.isComplete"
        active-text="Complete"
        inactive-text="Todo"
      />
    </el-form-item>

    <el-form-item label="Scheduled Date" prop="scheduledDateTime">
      <el-date-picker
        v-model="formData.scheduledDateTime"
        type="datetime"
        placeholder="Select scheduled date/time"
        format="MM/DD/YYYY HH:mm"
        value-format="YYYY-MM-DDTHH:mm:ss"
        clearable
      />
    </el-form-item>

    <el-form-item label="Due Date" prop="dueDateTime">
      <el-date-picker
        v-model="formData.dueDateTime"
        type="datetime"
        placeholder="Select due date/time"
        format="MM/DD/YYYY HH:mm"
        value-format="YYYY-MM-DDTHH:mm:ss"
        clearable
      />
    </el-form-item>

    <el-form-item>
      <el-button type="primary" @click="handleSubmit" :loading="loading">
        {{ todo ? 'Update' : 'Create' }}
      </el-button>
      <el-button @click="handleCancel">Cancel</el-button>
    </el-form-item>
  </el-form>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import type { TodoItem } from '@/types/todo'

interface Props {
  todo?: TodoItem | null
}

const props = withDefaults(defineProps<Props>(), {
  todo: null,
})

const emit = defineEmits<{
  submit: [todo: TodoItem]
  cancel: []
}>()

const formRef = ref<FormInstance>()
const loading = ref(false)

const formData = reactive<TodoItem>({
  title: '',
  description: '',
  isComplete: false,
  scheduledDateTime: null,
  dueDateTime: null,
})

const rules: FormRules = {
  title: [
    { required: true, message: 'Please enter a title', trigger: 'blur' },
    { min: 1, max: 100, message: 'Title should be 1-100 characters', trigger: 'blur' },
  ],
}

// Initialize form with existing todo data if provided
watch(() => props.todo, (newTodo) => {
  if (newTodo) {
    Object.assign(formData, {
      title: newTodo.title || '',
      description: newTodo.description || '',
      isComplete: newTodo.isComplete,
      scheduledDateTime: newTodo.scheduledDateTime,
      dueDateTime: newTodo.dueDateTime,
    })
  }
}, { immediate: true })

const handleSubmit = async () => {
  if (!formRef.value) return

  try {
    await formRef.value.validate()
    loading.value = true

    const todoData: TodoItem = {
      ...formData,
      id: props.todo?.id,
    }

    emit('submit', todoData)
  } catch (error) {
    console.error('Form validation failed:', error)
  } finally {
    loading.value = false
  }
}

const handleCancel = () => {
  emit('cancel')
}
</script>
