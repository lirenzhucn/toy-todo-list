<template>
  <el-table :data="todos" style="width: 100%" v-loading="loading">
    <el-table-column prop="title" label="Title" min-width="150" />
    <el-table-column prop="description" label="Description" min-width="200" />
    <el-table-column label="Status" width="100">
      <template #default="{ row }">
        <el-tag :type="row.isComplete ? 'success' : 'warning'">
          {{ row.isComplete ? 'Complete' : 'Incomplete' }}
        </el-tag>
      </template>
    </el-table-column>
    <el-table-column label="Scheduled" width="150">
      <template #default="{ row }">
        {{ formatDateTime(row.scheduledDateTime) }}
      </template>
    </el-table-column>
    <el-table-column label="Due" width="150">
      <template #default="{ row }">
        {{ formatDateTime(row.dueDateTime) }}
      </template>
    </el-table-column>
    <el-table-column label="Actions" width="150" fixed="right">
      <template #default="{ row }">
        <el-button 
          size="small" 
          @click="handleEdit(row)"
          :icon="Edit"
        />
        <el-button 
          size="small" 
          type="danger" 
          @click="handleDelete(row)"
          :icon="Delete"
        />
      </template>
    </el-table-column>
  </el-table>

  <!-- Edit Dialog -->
  <el-dialog v-model="showEditDialog" title="Edit Todo" width="500px">
    <TodoForm 
      :todo="editingTodo" 
      @submit="handleUpdateTodo" 
      @cancel="showEditDialog = false" 
    />
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Edit, Delete } from '@element-plus/icons-vue'
import dayjs from 'dayjs'
import type { TodoItem } from '@/types/todo'
import { useTodoStore } from '@/stores/todo'
import TodoForm from './TodoForm.vue'

interface Props {
  todos: TodoItem[]
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
})

const todoStore = useTodoStore()
const showEditDialog = ref(false)
const editingTodo = ref<TodoItem | null>(null)

const formatDateTime = (dateTime: string | null | undefined) => {
  if (!dateTime) return '-'
  return dayjs(dateTime).format('MM/DD/YYYY HH:mm')
}

const handleEdit = (todo: TodoItem) => {
  editingTodo.value = { ...todo }
  showEditDialog.value = true
}

const handleUpdateTodo = async (todo: TodoItem) => {
  if (editingTodo.value?.id) {
    await todoStore.updateTodo(editingTodo.value.id, todo)
    showEditDialog.value = false
    ElMessage.success('Todo updated successfully')
  }
}

const handleDelete = async (todo: TodoItem) => {
  try {
    await ElMessageBox.confirm(
      `Are you sure you want to delete "${todo.title}"?`,
      'Delete Todo',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    if (todo.id) {
      await todoStore.deleteTodo(todo.id)
      ElMessage.success('Todo deleted successfully')
    }
  } catch (error) {
    // User cancelled or error occurred
    console.log('Delete cancelled or failed:', error)
  }
}
</script>