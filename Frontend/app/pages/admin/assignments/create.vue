<script setup lang="ts">
import { z } from 'zod'
import { $authedFetch, handleResponseError } from '~/apis/api'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

const schema = z.object({
  title: z
    .string()
    .min(1, 'Title is required')
    .max(200, 'Title must be 200 characters or less'),
  description: z
    .string()
    .nonoptional(),
  dueDate: z.string().optional()
})

type AssignmentSchema = z.output<typeof schema>

const state = reactive({
  title: '',
  description: '',
  dueDate: ''
})

const editorRenderKey = ref(0)
const formLoading = ref(false)
const toast = useToast()
const router = useRouter()

async function onSubmit(event: { data: AssignmentSchema }) {
  try {
    formLoading.value = true
    const body = {
      title: event.data.title,
      description: event.data.description || null,
      dueDate: event.data.dueDate ? new Date(event.data.dueDate).toISOString() : null
    }
    await $authedFetch('/assignments', {
      method: 'POST',
      body
    })
    toast.add({
      title: 'Assignment created successfully',
      color: 'success'
    })
    router.push('/admin/assignments')
  } catch (error) {
    handleResponseError(error)
  } finally {
    formLoading.value = false
  }
}
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar title="Create Assignment" />
    </template>

    <template #body>
      <div>
        <div class="flex items-center gap-2 mb-6">
          <UButton
            icon="i-lucide-arrow-left"
            color="neutral"
            variant="ghost"
            to="/admin/assignments"
          />
          <h1 class="text-2xl font-bold">
            Create New Assignment
          </h1>
        </div>

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

          <UFormField
            label="Description"
            name="description"
          >
            <MarkdownEditor
              v-model="state.description"
              :render-key="editorRenderKey"
              placeholder="Enter assignment description (optional)"
            />
          </UFormField>

          <div class="flex justify-end gap-2 pt-4">
            <UButton
              color="neutral"
              variant="subtle"
              to="/admin/assignments"
            >
              Cancel
            </UButton>
            <UButton
              type="submit"
              :loading="formLoading"
            >
              Create
            </UButton>
          </div>
        </UForm>
      </div>
    </template>
  </UDashboardPanel>
</template>
