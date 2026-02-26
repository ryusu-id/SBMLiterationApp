<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { $authedFetch, handleResponseError } from '~/apis/api'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

const UBadge = resolveComponent('UBadge')

interface AssignmentSubmissionFile {
  id: number
  fileName: string
  fileUri?: string
  externalLink?: string
  uploadedAt: string
  uploadedByUser: { id: number, name: string }
}

interface AssignmentSubmission {
  id: number
  groupId: number
  group: { id: number, name: string }
  isCompleted: boolean
  completedAt?: string
  createTime: string
  files: AssignmentSubmissionFile[]
}

interface AssignmentDetail {
  id: number
  title: string
  description?: string
  dueDate?: string
  submissions: AssignmentSubmission[]
}

const route = useRoute()
const assignmentId = computed(() => Number(route.params.id))

const assignment = ref<AssignmentDetail | null>(null)
const loading = ref(false)

async function fetchAssignment() {
  loading.value = true
  try {
    const response = await $authedFetch<{ data: AssignmentDetail }>(`/assignments/${assignmentId.value}`)
    assignment.value = response?.data || null
  } catch (error) {
    handleResponseError(error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchAssignment()
})

const submissionColumns: TableColumn<AssignmentSubmission>[] = [
  {
    id: 'group',
    header: 'Group',
    cell: ({ row }) => row.original.group?.name || h('span', { class: 'text-muted' }, '-')
  },
  {
    id: 'status',
    header: 'Status',
    meta: { class: { th: 'w-[140px]' } },
    cell: ({ row }) => {
      return h(
        UBadge,
        {
          color: row.original.isCompleted ? 'success' : 'neutral',
          variant: 'subtle'
        },
        () => row.original.isCompleted ? 'Completed' : 'In Progress'
      )
    }
  },
  {
    id: 'files',
    header: 'Files',
    meta: { class: { th: 'w-[80px]' } },
    cell: ({ row }) => {
      const count = row.original.files?.length || 0
      return `${count} file${count !== 1 ? 's' : ''}`
    }
  },
  {
    id: 'completedAt',
    header: 'Completed At',
    meta: { class: { th: 'w-[180px]' } },
    cell: ({ row }) => {
      if (!row.original.completedAt) return h('span', { class: 'text-muted' }, '-')
      return new Date(row.original.completedAt).toLocaleString()
    }
  }
]
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar :title="assignment?.title || 'Assignment Detail'" />
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
            {{ assignment?.title || 'Loading...' }}
          </h1>
        </div>

        <div
          v-if="loading"
          class="flex justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <div
          v-else-if="assignment"
          class="space-y-6"
        >
          <UCard>
            <template #header>
              <h2 class="text-lg font-semibold">
                Assignment Info
              </h2>
            </template>
            <div class="space-y-3">
              <div class="flex flex-row flex-wrap justify-between">
                <div>
                  <p class="text-2xl mb-2 text-muted">
                    Title
                  </p>
                  <p class="font-medium">
                    {{ assignment.title }}
                  </p>
                </div>
                <div>
                  <p class="text-2xl mb-2 text-muted">
                    Due Date
                  </p>
                  <p class="font-medium">
                    {{ assignment.dueDate ? new Date(assignment.dueDate).toLocaleString() : 'No due date' }}
                  </p>
                </div>
              </div>
              <div v-if="assignment.description">
                <p class="text-2xl mb-2 text-muted">
                  Description
                </p>
                <UEditor
                  :model-value="assignment.description"
                  content-type="markdown"
                  readonly
                  :editable="false"
                  class="custom-prose"
                  :ui="{
                    content: 'p-0 sm:px-0'
                  }"
                />
              </div>
            </div>
          </UCard>

          <UCard>
            <template #header>
              <h2 class="text-lg font-semibold">
                Group Submissions ({{ assignment.submissions?.length || 0 }})
              </h2>
            </template>
            <UTable
              :data="assignment.submissions || []"
              :columns="submissionColumns"
              class="w-full"
            />
          </UCard>
        </div>
      </div>
    </template>
  </UDashboardPanel>
</template>

<style scoped>
.custom-prose :deep(.ProseMirror) {
    padding: 0
}
</style>
