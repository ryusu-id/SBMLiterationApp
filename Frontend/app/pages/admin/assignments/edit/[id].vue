<script setup lang="ts">
import { z } from 'zod'
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

interface AssignmentDetail {
  id: number
  title: string
  description?: string
  dueDate?: string
}

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

const route = useRoute()
const router = useRouter()
const assignmentId = computed(() => Number(route.params.id))

const state = reactive({
  title: '',
  description: '',
  dueDate: ''
})

const editorRenderKey = ref(0)
const pending = ref(false)
const formLoading = ref(false)
const toast = useToast()

async function fetchAssignment() {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<AssignmentDetail>>(`/assignments/${assignmentId.value}`)
    if (response.data) {
      state.title = response.data.title
      state.description = response.data.description || ''
      state.dueDate = response.data.dueDate ? response.data.dueDate.slice(0, 16) : ''
      editorRenderKey.value++
    } else {
      handleResponseError(response)
      router.push('/admin/assignments')
    }
  } catch (err) {
    handleResponseError(err)
    router.push('/admin/assignments')
  } finally {
    pending.value = false
  }
}

onMounted(() => {
  fetchAssignment()
})

async function onSubmit(event: { data: AssignmentSchema }) {
  try {
    formLoading.value = true
    const body = {
      title: event.data.title,
      description: event.data.description || null,
      dueDate: event.data.dueDate ? new Date(event.data.dueDate).toISOString() : null
    }
    await $authedFetch(`/assignments/${assignmentId.value}`, {
      method: 'PUT',
      body
    })
    toast.add({
      title: 'Assignment updated successfully',
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
      <DashboardNavbar title="Edit Assignment" />
    </template>

    <template #body>
      <div
        v-if="pending"
        class="flex justify-center py-12"
      >
        <UIcon
          name="i-heroicons-arrow-path"
          class="animate-spin text-4xl"
        />
      </div>

      <div v-else>
        <div class="flex items-center gap-2 mb-6">
          <UButton
            icon="i-lucide-arrow-left"
            color="neutral"
            variant="ghost"
            to="/admin/assignments"
          />
          <h1 class="text-2xl font-bold">
            Edit Assignment
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
              Update
            </UButton>
          </div>
        </UForm>
      </div>
    </template>
  </UDashboardPanel>
</template>
