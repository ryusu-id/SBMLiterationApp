<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'

defineProps<{
  loading?: boolean
}>()

const schema = z.object({
  categoryName: z
    .string()
    .min(1, 'Category name is required')
    .max(50, 'Category name must be 50 characters or less')
})

export type ReadingCategorySchema = z.output<typeof schema>

const state = reactive({
  categoryName: ''
})

const id = ref<number | null>(null)

const emit = defineEmits<{
  (e: 'submit', data: { action: 'Create' | 'Update', data: ReadingCategorySchema, id: number | null }): void
}>()

const action = ref<'Create' | 'Update'>('Create')
const open = ref(false)
function create() {
  action.value = 'Create'
  id.value = null
  state.categoryName = ''
  open.value = true
}

function update(param: { id: number, categoryName: string }) {
  action.value = 'Update'
  id.value = param.id
  state.categoryName = param.categoryName
  open.value = true
}

defineExpose({
  create,
  update,
  close: () => {
    open.value = false
  }
})

async function onSubmit(event: FormSubmitEvent<ReadingCategorySchema>) {
  emit('submit', { action: action.value, data: event.data, id: id.value })
}
</script>

<template>
  <UModal
    v-model:open="open"
    :title="action + ' Reading Category'"
    :description="`Fill in the form below to ${action.toLowerCase()} reading categories`"
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
          label="Category Name"
          name="categoryName"
          required
        >
          <UInput
            v-model="state.categoryName"
            placeholder="Enter category name"
            size="lg"
            maxlength="50"
            class="w-full"
          />
        </UFormField>

        <div class="flex justify-end pt-4">
          <div>
            <UButton
              type="submit"
              :loading
            >
              Save
            </UButton>
          </div>
        </div>
      </UForm>
    </template>
  </UModal>
</template>
