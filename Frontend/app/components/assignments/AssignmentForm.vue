<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'

defineProps<{
  loading?: boolean
}>()

const schema = z.object({
  title: z
    .string()
    .min(1, 'Title is required')
    .max(200, 'Title must be 200 characters or less'),
  description: z
    .string()
    .max(2000, 'Description must be 2000 characters or less')
    .optional(),
  dueDate: z.string().optional()
})

export type AssignmentSchema = z.output<typeof schema>

const state = reactive({
  title: '',
  description: '',
  dueDate: ''
})

const id = ref<number | null>(null)

const emit = defineEmits<{
  (e: 'submit', data: { action: 'Create' | 'Update', data: AssignmentSchema, id: number | null }): void
}>()

const action = ref<'Create' | 'Update'>('Create')
const open = ref(false)

function create() {
  action.value = 'Create'
  id.value = null
  state.title = ''
  state.description = ''
  state.dueDate = ''
  open.value = true
}

function update(param: { id: number, title: string, description?: string, dueDate?: string }) {
  action.value = 'Update'
  id.value = param.id
  state.title = param.title
  state.description = param.description || ''
  state.dueDate = param.dueDate ? param.dueDate.slice(0, 16) : ''
  open.value = true
}

defineExpose({
  create,
  update,
  close: () => {
    open.value = false
  }
})

async function onSubmit(event: FormSubmitEvent<AssignmentSchema>) {
  emit('submit', {
    action: action.value,
    data:
    {
      ...event.data,
      dueDate: event.data.dueDate ? new Date(event.data.dueDate).toISOString() : undefined
    },
    id: id.value
  })
}
</script>

<template>
  <UModal
    v-model:open="open"
    :title="action + ' Assignment'"
    :description="`Fill in the form below to ${action.toLowerCase()} an assignment`"
    :close="{
      variant: 'subtle'
    }"
  >
    <template #body>
      <UForm
        :schema="schema"
        :state="state"
        class="space-y-6"
        @submit="onSubmit"
      >
        <UFormField
          label="Title"
          name="title"
          required
        >
          <UInput
            v-model="state.title"
            placeholder="Enter assignment title"
            size="lg"
            maxlength="200"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Description"
          name="description"
        >
          <UTextarea
            v-model="state.description"
            placeholder="Enter assignment description (optional)"
            :rows="4"
            maxlength="2000"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Due Date"
          name="dueDate"
        >
          <UInput
            v-model="state.dueDate"
            type="datetime-local"
            size="lg"
            class="w-full"
          />
        </UFormField>

        <div class="flex justify-end pt-4">
          <UButton
            type="submit"
            :loading
          >
            Save
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>
