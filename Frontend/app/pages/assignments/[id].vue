<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

// Assignment metadata from GET /api/assignments/{id}
interface AssignmentInfo {
  id: number
  title: string
  description?: string
  dueDate?: string
  createTime: string
  updateTime?: string
}

// File shape from GET /api/assignments/{assignmentId}/submission/my
// uploadedBy is a flat string — NOT a nested object
interface SubmissionFileDetail {
  fileId: number
  fileName: string
  fileUri?: string
  externalLink?: string
  uploadedByUserId: number
  uploadedByName: string
  uploadedAt: string
}

// Submission shape from GET /api/assignments/{assignmentId}/submission/my
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

function timeLeft(dueDate: string): { label: string, overdue: boolean } {
  const diff = new Date(dueDate).getTime() - Date.now()
  if (diff <= 0) return { label: 'Overdue', overdue: true }
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(minutes / 60)
  const days = Math.floor(hours / 24)
  if (days > 0) return { label: `${days} day${days > 1 ? 's' : ''} left`, overdue: false }
  if (hours > 0) return { label: `${hours} hour${hours > 1 ? 's' : ''} left`, overdue: false }
  return { label: `${minutes} minute${minutes > 1 ? 's' : ''} left`, overdue: false }
}

const route = useRoute()
const assignmentId = computed(() => Number(route.params.id))

const assignment = ref<AssignmentInfo | null>(null)
const submission = ref<SubmissionDetail | null>(null)
const loading = ref(false)
const toast = useToast()

const addFileOpen = ref(false)
const addFileType = ref<'link' | 'file'>('link')
const addFileName = ref('')
const addFileUrl = ref('')
const addFilePicked = ref<File | null>(null)
const addFileLoading = ref(false)

const completeLoading = ref(false)

const isOverDue = computed(() => {
  if (!assignment.value?.dueDate) return false
  return new Date(assignment.value.dueDate).getTime() < Date.now()
})

async function fetchAssignment() {
  try {
    const [assignmentRes, submissionRes] = await Promise.all([
      $authedFetch<{ data: AssignmentInfo }>(
        `/assignments/${assignmentId.value}`
      ),
      $authedFetch<{ data: SubmissionDetail | null }>(
        `/assignments/${assignmentId.value}/submission/my`
      )
    ])
    assignment.value = assignmentRes?.data || null
    submission.value = submissionRes?.data || null
  } catch (error) {
    handleResponseError(error)
  } finally {
    loading.value = false
  }
}

async function fetchSubmission() {
  try {
    const submissionRes = await $authedFetch<{ data: SubmissionDetail | null }>(
      `/assignments/${assignmentId.value}/submission/my`
    )
    submission.value = submissionRes?.data || null
  } catch (error) {
    handleResponseError(error)
  }
}

onMounted(() => {
  fetchAssignment()
})

function openAddFile() {
  addFileType.value = 'link'
  addFileName.value = ''
  addFileUrl.value = ''
  addFilePicked.value = null
  addFileOpen.value = true
}

function handleFileInput(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files?.[0]) {
    addFilePicked.value = target.files[0]
    if (!addFileName.value) {
      addFileName.value = target.files[0].name
    }
  }
}

async function submitAddFile() {
  if (!addFileName.value.trim()) return

  try {
    addFileLoading.value = true
    const formData = new FormData()
    formData.append('fileName', addFileName.value)
    formData.append('externalLink', addFileUrl.value)
    if (addFileType.value === 'link') {
      await $authedFetch(
        `/assignments/${assignmentId.value}/submission/my/files`,
        {
          method: 'POST',
          body: formData
        }
      )
    } else if (addFilePicked.value) {
      const formData = new FormData()
      formData.append('file', addFilePicked.value)
      formData.append('fileName', addFileName.value)
      await $authedFetch(
        `/assignments/${assignmentId.value}/submission/my/files`,
        {
          method: 'POST',
          body: formData
        }
      )
    }
    toast.add({ title: 'File added successfully', color: 'success' })
    addFileOpen.value = false
    fetchSubmission()
  } catch (error) {
    handleResponseError(error)
  } finally {
    addFileLoading.value = false
  }
}

async function removeFile(fileId: number) {
  const dialog = useDialog()
  dialog.confirm({
    title: 'Remove File',
    message: 'Are you sure you want to remove this file?',
    subTitle: 'This action cannot be undone.',
    async onOk() {
      try {
        await $authedFetch(
          `/assignments/${assignmentId.value}/submission/my/files/${fileId}`,
          {
            method: 'DELETE'
          }
        )
        toast.add({ title: 'File removed successfully', color: 'success' })
        fetchSubmission()
      } catch (error) {
        handleResponseError(error)
      }
    }
  })
}

async function toggleComplete() {
  try {
    completeLoading.value = true
    if (submission.value?.isCompleted) {
      await $authedFetch(
        `/assignments/${assignmentId.value}/submission/my/complete`,
        {
          method: 'DELETE'
        }
      )
      toast.add({ title: 'Submission unmarked as complete', color: 'neutral' })
    } else {
      await $authedFetch(
        `/assignments/${assignmentId.value}/submission/my/complete`,
        {
          method: 'POST',
          body: {
            assignmentId: assignmentId.value
          }
        }
      )
      toast.add({ title: 'Submission marked as complete!', color: 'success' })
    }
    fetchAssignment()
  } catch (error) {
    handleResponseError(error)
  } finally {
    completeLoading.value = false
  }
}
</script>

<template>
  <UContainer class="py-8">
    <div class="flex flex-col space-y-6">
      <div class="flex items-center gap-2">
        <UButton
          icon="i-lucide-arrow-left"
          color="neutral"
          variant="ghost"
          to="/assignments"
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
        class="flex flex-col lg:flex-row gap-6 items-start w-full"
      >
        <!-- Left: Assignment Details -->
        <div class="flex-1 w-full">
          <UCard>
            <template #header>
              <div class="flex flex-col sm:flex-row sm:justify-between gap-2">
                <h2 class="text-lg font-semibold">
                  Assignment Details
                </h2>

                <div>
                  <p class="text-sm text-muted">
                    Due Date
                  </p>
                  <p class="font-medium">
                    {{
                      assignment.dueDate
                        ? new Date(assignment.dueDate).toLocaleString()
                        : "No due date"
                    }}
                  </p>
                  <p
                    v-if="assignment.dueDate"
                    :class="timeLeft(assignment.dueDate).overdue ? 'text-red-500' : 'text-green-500'"
                    class="text-xs mt-0.5"
                  >
                    {{ timeLeft(assignment.dueDate).label }}
                  </p>
                </div>
              </div>
            </template>
            <div class="space-y-3">
              <div v-if="assignment.description">
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
        </div>

        <!-- Right: Assignment Actions -->
        <div class="w-full lg:w-80 shrink-0">
          <UCard>
            <template #header>
              <div class="flex items-center justify-between">
                <h2 class="text-sm font-semibold flex-2">
                  Your Group's Submission
                </h2>
                <UBadge
                  :color="submission?.isCompleted ? 'success' : 'neutral'"
                  variant="subtle"
                >
                  {{ submission?.isCompleted ? "Submitted" : "Not Submitted" }}
                </UBadge>
              </div>
            </template>

            <div class="space-y-4">
              <!-- Files section -->
              <div>
                <div class="flex items-center justify-between mb-3">
                  <h3 class="font-medium">
                    Attached Files
                  </h3>
                  <UButton
                    v-if="!isOverDue"
                    icon="i-lucide-plus"
                    size="sm"
                    color="neutral"
                    variant="subtle"
                    label="Add File"
                    :disabled="submission?.isCompleted || isOverDue"
                    @click="openAddFile"
                  />
                </div>

                <div
                  v-if="!submission?.files?.length"
                  class="text-sm text-muted py-6 text-center border border-dashed border-default rounded-lg"
                >
                  No files attached yet
                </div>

                <div
                  v-else
                  class="space-y-2"
                >
                  <div
                    v-for="file in submission?.files"
                    :key="file.fileId"
                    class="flex items-center justify-between p-3 rounded-lg border border-default"
                  >
                    <div class="flex items-center gap-2 flex-1 min-w-0">
                      <UIcon
                        :name="file.externalLink ? 'i-lucide-link' : 'i-lucide-file'"
                        class="shrink-0 text-muted"
                      />
                      <div class="min-w-0">
                        <p class="text-sm font-medium truncate">
                          {{ file.fileName }}
                        </p>
                        <p class="text-xs text-muted">
                          by {{ file.uploadedByName }} ·
                          {{ new Date(file.uploadedAt).toLocaleDateString() }}
                        </p>
                      </div>
                    </div>
                    <div class="flex items-center gap-1 shrink-0">
                      <UButton
                        v-if="file.externalLink"
                        icon="i-lucide-external-link"
                        size="xs"
                        color="primary"
                        variant="ghost"
                        :to="file.externalLink"
                        target="_blank"
                      />
                      <UButton
                        v-if="file.fileUri"
                        icon="i-lucide-download"
                        size="xs"
                        color="primary"
                        variant="ghost"
                        :to="file.fileUri"
                        target="_blank"
                      />
                      <UButton
                        v-if="!isOverDue"
                        icon="i-lucide-trash"
                        size="xs"
                        color="error"
                        variant="ghost"
                        :disabled="submission?.isCompleted"
                        @click="removeFile(file.fileId)"
                      />
                    </div>
                  </div>
                </div>
              </div>

              <!-- Mark complete section -->
              <div class="border-t border-default pt-4">
                <div class="flex flex-col items-end gap-y-2 justify-between">
                  <div
                    v-if="submission?.completedAt"
                  >
                    <p class="font-medium text-right">
                      Submission Status
                    </p>
                    <p class="text-sm text-muted">
                      Completed at
                      {{ new Date(submission?.completedAt!).toLocaleString() }}
                    </p>
                  </div>
                  <UButton
                    v-if="!isOverDue"
                    :icon="
                      submission?.isCompleted
                        ? 'i-lucide-x-circle'
                        : 'i-lucide-check-circle'
                    "
                    :color="submission?.isCompleted ? 'neutral' : 'primary'"
                    :variant="submission?.isCompleted ? 'subtle' : 'solid'"
                    :loading="completeLoading"
                    :label="
                      submission?.isCompleted
                        ? 'Unmark Complete'
                        : 'Mark as Complete'
                    "
                    @click="toggleComplete"
                  />
                </div>
              </div>
            </div>
          </UCard>
        </div>
      </div>
    </div>
    <UModal
      v-model:open="addFileOpen"
      title="Add File"
      description="Attach a file or external link to your submission"
      :close="{ variant: 'subtle' }"
    >
      <template #body>
        <div class="space-y-4">
          <UFormField
            label="Display Name"
            required
          >
            <UInput
              v-model="addFileName"
              placeholder="Enter a display name for this file"
              class="w-full"
            />
          </UFormField>

          <div class="flex gap-2">
            <UButton
              :color="addFileType === 'link' ? 'primary' : 'neutral'"
              :variant="addFileType === 'link' ? 'solid' : 'subtle'"
              icon="i-lucide-link"
              label="External Link"
              size="sm"
              @click="addFileType = 'link'"
            />
            <UButton
              :color="addFileType === 'file' ? 'primary' : 'neutral'"
              :variant="addFileType === 'file' ? 'solid' : 'subtle'"
              icon="i-lucide-upload"
              label="Upload File"
              size="sm"
              @click="addFileType = 'file'"
            />
          </div>

          <UFormField
            v-if="addFileType === 'link'"
            label="URL"
            required
          >
            <UInput
              v-model="addFileUrl"
              placeholder="https://..."
              class="w-full"
            />
          </UFormField>

          <UFormField
            v-else
            label="File"
            required
          >
            <input
              type="file"
              class="block w-full text-sm"
              @change="handleFileInput"
            >
          </UFormField>

          <div class="flex justify-end gap-2">
            <UButton
              color="neutral"
              variant="subtle"
              @click="addFileOpen = false"
            >
              Cancel
            </UButton>
            <UButton
              :loading="addFileLoading"
              :disabled="
                !addFileName.trim()
                  || (addFileType === 'link' ? !addFileUrl.trim() : !addFilePicked)
              "
              @click="submitAddFile"
            >
              Add
            </UButton>
          </div>
        </div>
      </template>
    </UModal>
  </UContainer>
</template>

<style scoped>
.custom-prose :deep(.ProseMirror) {
  padding: 0;
}
</style>
