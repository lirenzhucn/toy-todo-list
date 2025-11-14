<template>
  <div class="todo-list">
    <el-row :gutter="20">
      <el-col :span="24">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>Todo List</span>
              <el-button type="primary" @click="showAddDialog = true">
                <el-icon><Plus /></el-icon>
                Add Todo
              </el-button>
            </div>
          </template>

          <div v-if="todoStore.loading" class="loading">
            <el-loading text="Loading todos..." />
          </div>

          <div v-else-if="todoStore.error" class="error">
            <el-alert :title="todoStore.error" type="error" />
          </div>

          <div v-else>
            <el-tabs v-model="activeTab">
              <el-tab-pane label="Incomplete" name="incomplete">
                <TodoTable :todos="todoStore.incompleteTodos" />
              </el-tab-pane>
              <el-tab-pane label="Today" name="today">
                <TodoTable :todos="todoStore.todayTodos" />
              </el-tab-pane>
              <el-tab-pane label="All" name="all">
                <TodoTable :todos="todoStore.todos" />
              </el-tab-pane>
              <el-tab-pane label="Completed" name="completed">
                <TodoTable :todos="todoStore.completedTodos" />
              </el-tab-pane>
              <el-tab-pane label="Overdue" name="overdue">
                <TodoTable :todos="todoStore.overdueTodos" />
              </el-tab-pane>
            </el-tabs>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Add Todo Dialog -->
    <el-dialog v-model="showAddDialog" title="Add New Todo" width="500px">
      <TodoForm @submit="handleAddTodo" @cancel="showAddDialog = false" />
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useTodoStore } from '@/stores/todo'
import TodoTable from '@/components/TodoTable.vue'
import TodoForm from '@/components/TodoForm.vue'
import { Plus } from '@element-plus/icons-vue'

const todoStore = useTodoStore()
const activeTab = ref('incomplete')
const showAddDialog = ref(false)

onMounted(() => {
  todoStore.fetchTodos()
})

const handleAddTodo = async (todo: any) => {
  await todoStore.addTodo(todo)
  showAddDialog.value = false
}
</script>

<style scoped>
.todo-list {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.loading {
  text-align: center;
  padding: 40px;
}

.error {
  margin: 20px 0;
}
</style>