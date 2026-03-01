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
const UButton = resolveComponent('UButton')

// Shape from GET /api/assignments/{id}
interface AssignmentDetail {
  id: number
  title: string
  description?: string
  dueDate?: string
  createTime: string
  updateTime?: string
}

// Shape from GET /api/assignments/{id}/submissions (PagingResult<SubmissionListItem>.rows)
interface SubmissionListItem {
  submissionId?: number
  groupId: number
  groupName: string
  isCompleted: boolean
  completedAt?: string
  fileCount: number
  createTime?: string
}

// Shape from GET /api/assignments/{id}/submissions/{submissionId}
interface SubmissionFileDetail {
  fileId: number
  fileName: string
  fileUri?: string
  externalLink?: string
  uploadedByUserId: number
  uploadedByName: string
  uploadedAt: string
}

interface SubmissionDetail {
  submissionId: number
  assignmentId: number
  groupId: number
  groupName: string
  isCompleted: boolean
  completedAt?: string
  createTime: string
  files: SubmissionFileDetail[]
}

const route = useRoute()
const assignmentId = computed(() => Number(route.params.id))

const assignment = ref<AssignmentDetail | null>(null)
const submissions = ref<SubmissionListItem[]>([])
const loading = ref(false)

// File viewer modal state
const filesModalOpen = ref(false)
const selectedSubmission = ref<SubmissionDetail | null>(null)
const filesLoading = ref(false)

async function fetchAssignment() {
  loading.value = true
  try {
    const [assignmentRes, submissionsRes] = await Promise.all([
      $authedFetch<{ data: AssignmentDetail }>(
        `/assignments/${assignmentId.value}`
      ),
      $authedFetch<{ rows: SubmissionListItem[] }>(
        `/assignments/${assignmentId.value}/submissions`
      )
    ])
    assignment.value = assignmentRes?.data || null
    submissions.value = submissionsRes?.rows || []
  } catch (error) {
    handleResponseError(error)
  } finally {
    loading.value = false
  }
}

async function openFilesModal(submissionId: number) {
  selectedSubmission.value = null
  filesModalOpen.value = true
  filesLoading.value = true
  try {
    const res = await $authedFetch<{ data: SubmissionDetail }>(
      `/assignments/${assignmentId.value}/submissions/${submissionId}`
    )
    selectedSubmission.value = res?.data || null
  } catch (error) {
    handleResponseError(error)
    filesModalOpen.value = false
  } finally {
    filesLoading.value = false
  }
}

onMounted(() => {
  fetchAssignment()
})

type SubmissionStatus = 'Completed' | 'In Progress' | 'Not Yet'

function getStatus(row: SubmissionListItem): SubmissionStatus {
  if (row.isCompleted) return 'Completed'
  if (row.fileCount > 0) return 'In Progress'
  return 'Not Yet'
}

const statusColor: Record<SubmissionStatus, string> = {
  'Completed': 'success',
  'In Progress': 'warning',
  'Not Yet': 'error'
}

const submissionColumns: TableColumn<SubmissionListItem>[] = [
  {
    id: 'action',
    header: 'Action',
    meta: { class: { th: 'w-[80px]' } },
    cell: ({ row }) => {
      if (!row.original.submissionId) {
        return h('span', { class: 'text-muted text-xs' }, '—')
      }
      return h(UButton, {
        icon: 'i-lucide-download',
        size: 'xs',
        color: 'neutral',
        variant: 'subtle',
        onClick: () => openFilesModal(row.original.submissionId!)
      })
    }
  },
  {
    id: 'group',
    header: 'Group',
    cell: ({ row }) =>
      row.original.groupName || h('span', { class: 'text-muted' }, '-')
  },
  {
    id: 'status',
    header: 'Group Submit Status',
    meta: { class: { th: 'w-[180px]' } },
    cell: ({ row }) => {
      const status = getStatus(row.original)
      return h(
        UBadge,
        { color: statusColor[status], variant: 'subtle' },
        () => status
      )
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
            {{ assignment?.title || "Loading..." }}
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
                    {{
                      assignment.dueDate
                        ? new Date(assignment.dueDate).toLocaleString()
                        : "No due date"
                    }}
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
                Group Submissions ({{ submissions.length }})
              </h2>
            </template>
            <UTable
              :data="submissions"
              :columns="submissionColumns"
              class="w-full"
            />
          </UCard>
        </div>
      </div>

      <!-- File Viewer Modal -->
      <UModal
        v-model:open="filesModalOpen"
        :title="
          selectedSubmission
            ? `${selectedSubmission.groupName} — Files`
            : 'Loading...'
        "
        :close="{ variant: 'subtle' }"
      >
        <template #body>
          <div class="space-y-3">
            <div
              v-if="filesLoading"
              class="flex justify-center py-8"
            >
              <UIcon
                name="i-heroicons-arrow-path"
                class="animate-spin text-3xl"
              />
            </div>

            <template v-else-if="selectedSubmission">
              <div
                v-if="!selectedSubmission.files.length"
                class="text-sm text-muted text-center py-6 border border-dashed border-default rounded-lg"
              >
                No files attached
              </div>

              <div
                v-for="file in selectedSubmission.files"
                :key="file.fileId"
                class="flex items-start justify-between gap-3 p-3 rounded-lg border border-default"
              >
                <div class="flex items-start gap-2 flex-1 min-w-0">
                  <UIcon
                    :name="file.externalLink ? 'i-lucide-link' : 'i-lucide-file'"
                    class="shrink-0 text-muted mt-0.5"
                  />
                  <div class="min-w-0">
                    <p class="text-sm font-medium truncate">
                      {{ file.fileName }}
                    </p>
                    <p class="text-xs text-muted mt-0.5">
                      by {{ file.uploadedByName }} ·
                      {{ new Date(file.uploadedAt).toLocaleDateString() }}
                    </p>
                  </div>
                </div>
                <UButton
                  icon="i-lucide-external-link"
                  size="xs"
                  color="primary"
                  variant="subtle"
                  label="Open"
                  :to="file.fileUri ?? file.externalLink ?? '#'"
                  target="_blank"
                  class="shrink-0"
                />
              </div>
            </template>
          </div>
        </template>
      </UModal>
    </template>
  </UDashboardPanel>
</template>

<style scoped>
.custom-prose :deep(.ProseMirror) {
  padding: 0;
}
</style>
