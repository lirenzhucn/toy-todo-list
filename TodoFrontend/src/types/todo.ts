export interface TodoItem {
  id?: number
  title?: string | null
  description?: string | null
  isComplete: boolean
  scheduledDateTime?: string | null
  dueDateTime?: string | null
}

export interface TodoFilters {
  scheduledDateTimeFrom?: string
  scheduledDateTimeTo?: string
  dueDateTimeFrom?: string
  dueDateTimeTo?: string
}