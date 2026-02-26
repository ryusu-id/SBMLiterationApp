<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'

defineProps<{
  loading?: boolean
}>()

const schema = z.object({
  name: z
    .string()
    .min(1, 'Group name is required')
    .max(100, 'Group name must be 100 characters or less'),
  description: z
    .string()
    .max(500, 'Description must be 500 characters or less')
    .optional()
})

export type GroupSchema = z.output<typeof schema>

const state = reactive({
  name: '',
  description: ''
})

const id = ref<number | null>(null)

const emit = defineEmits<{
  (e: 'submit', data: { action: 'Create' | 'Update', data: GroupSchema, id: number | null }): void
}>()

const action = ref<'Create' | 'Update'>('Create')
const open = ref(false)

function create() {
  action.value = 'Create'
  id.value = null
  state.name = ''
  state.description = ''
  open.value = true
}

function update(param: { id: number, name: string, description?: string }) {
  action.value = 'Update'
  id.value = param.id
  state.name = param.name
  state.description = param.description || ''
  open.value = true
}

defineExpose({
  create,
  update,
  close: () => {
    open.value = false
  }
})

async function onSubmit(event: FormSubmitEvent<GroupSchema>) {
  emit('submit', { action: action.value, data: event.data, id: id.value })
}
</script>

<template>
  <UModal
    v-model:open="open"
    :title="action + ' Group'"
    :description="`Fill in the form below to ${action.toLowerCase()} a group`"
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
          label="Name"
          name="name"
          required
        >
          <UInput
            v-model="state.name"
            placeholder="Enter group name"
            size="lg"
            maxlength="100"
            class="w-full"
          />
        </UFormField>

        <UFormField
          v-if="action === 'Create'"
          label="Description"
          name="description"
        >
          <UInput
            v-model="state.description"
            placeholder="Enter group description (optional)"
            size="lg"
            maxlength="500"
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
