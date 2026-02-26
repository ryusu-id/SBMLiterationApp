<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

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
  isCompleted: boolean
  completedAt?: string
  files: AssignmentSubmissionFile[]
}

interface MyAssignment {
  id: number
  title: string
  description?: string
  dueDate?: string
  submission?: AssignmentSubmission
}

const route = useRoute()
const assignmentId = computed(() => Number(route.params.id))

const assignment = ref<MyAssignment | null>(null)
const loading = ref(false)
const toast = useToast()

const addFileOpen = ref(false)
const addFileType = ref<'link' | 'file'>('link')
const addFileName = ref('')
const addFileUrl = ref('')
const addFilePicked = ref<File | null>(null)
const addFileLoading = ref(false)

const completeLoading = ref(false)

async function fetchAssignment() {
  loading.value = true
  try {
    const response = await $authedFetch<{ data: MyAssignment[] }>('/assignments/my')
    const list = response?.data || []
    assignment.value = list.find(a => a.id === assignmentId.value) || null
  } catch (error) {
    handleResponseError(error)
  } finally {
    loading.value = false
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
  const submission = assignment.value?.submission
  if (!submission) return

  try {
    addFileLoading.value = true
    if (addFileType.value === 'link') {
      await $authedFetch(`/assignments/${assignmentId.value}/submissions/${submission.id}/files`, {
        method: 'POST',
        body: {
          fileName: addFileName.value,
          externalLink: addFileUrl.value
        }
      })
    } else if (addFilePicked.value) {
      const formData = new FormData()
      formData.append('file', addFilePicked.value)
      formData.append('fileName', addFileName.value)
      await $authedFetch(`/assignments/${assignmentId.value}/submissions/${submission.id}/files`, {
        method: 'POST',
        body: formData
      })
    }
    toast.add({ title: 'File added successfully', color: 'success' })
    addFileOpen.value = false
    fetchAssignment()
  } catch (error) {
    handleResponseError(error)
  } finally {
    addFileLoading.value = false
  }
}

async function removeFile(fileId: number) {
  const submission = assignment.value?.submission
  if (!submission) return

  const dialog = useDialog()
  dialog.confirm({
    title: 'Remove File',
    message: 'Are you sure you want to remove this file?',
    subTitle: 'This action cannot be undone.',
    async onOk() {
      try {
        await $authedFetch(`/assignments/${assignmentId.value}/submissions/${submission.id}/files/${fileId}`, {
          method: 'DELETE'
        })
        toast.add({ title: 'File removed successfully', color: 'success' })
        fetchAssignment()
      } catch (error) {
        handleResponseError(error)
      }
    }
  })
}

async function toggleComplete() {
  const submission = assignment.value?.submission
  if (!submission) return

  try {
    completeLoading.value = true
    if (submission.isCompleted) {
      await $authedFetch(`/assignments/${assignmentId.value}/submissions/${submission.id}/complete`, {
        method: 'DELETE'
      })
      toast.add({ title: 'Submission unmarked as complete', color: 'neutral' })
    } else {
      await $authedFetch(`/assignments/${assignmentId.value}/submissions/${submission.id}/complete`, {
        method: 'POST'
      })
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
        <!-- Assignment Info -->
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
                  {{ assignment.dueDate ? new Date(assignment.dueDate).toLocaleString() : 'No due date' }}
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

        <!-- Submission -->
        <UCard>
          <template #header>
            <div class="flex items-center justify-between">
              <h2 class="text-lg font-semibold">
                Your Group's Submission
              </h2>
              <UBadge
                :color="assignment.submission?.isCompleted ? 'success' : 'neutral'"
                variant="subtle"
              >
                {{ assignment.submission?.isCompleted ? 'Completed' : 'In Progress' }}
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
                  icon="i-lucide-plus"
                  size="sm"
                  color="neutral"
                  variant="subtle"
                  label="Add File"
                  :disabled="assignment.submission?.isCompleted"
                  @click="openAddFile"
                />
              </div>

              <div
                v-if="!assignment.submission?.files?.length"
                class="text-sm text-muted py-6 text-center border border-dashed border-default rounded-lg"
              >
                No files attached yet
              </div>

              <div
                v-else
                class="space-y-2"
              >
                <div
                  v-for="file in assignment.submission?.files"
                  :key="file.id"
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
                        by {{ file.uploadedByUser?.name }} · {{ new Date(file.uploadedAt).toLocaleDateString() }}
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
                      icon="i-lucide-trash"
                      size="xs"
                      color="error"
                      variant="ghost"
                      :disabled="assignment.submission?.isCompleted"
                      @click="removeFile(file.id)"
                    />
                  </div>
                </div>
              </div>
            </div>

            <!-- Mark complete section -->
            <div class="border-t border-default pt-4">
              <div class="flex items-center justify-between">
                <div>
                  <p class="font-medium">
                    Submission Status
                  </p>
                  <p
                    v-if="assignment.submission?.completedAt"
                    class="text-sm text-muted"
                  >
                    Completed at {{ new Date(assignment.submission?.completedAt).toLocaleString() }}
                  </p>
                </div>
                <UButton
                  :icon="assignment.submission?.isCompleted ? 'i-lucide-x-circle' : 'i-lucide-check-circle'"
                  :color="assignment.submission?.isCompleted ? 'neutral' : 'success'"
                  :variant="assignment.submission?.isCompleted ? 'subtle' : 'solid'"
                  :loading="completeLoading"
                  :label="assignment.submission?.isCompleted ? 'Unmark Complete' : 'Mark as Complete'"
                  @click="toggleComplete"
                />
              </div>
            </div>
          </div>
        </UCard>
      </div>

      <!-- Add File Modal -->
      <UModal
        v-model:open="addFileOpen"
        title="Add File"
        description="Attach a file or external link to your submission"
        :close="{
          variant: 'subtle'
        }"
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
                :disabled="!addFileName.trim() || (addFileType === 'link' ? !addFileUrl.trim() : !addFilePicked)"
                @click="submitAddFile"
              >
                Add
              </UButton>
            </div>
          </div>
        </template>
      </UModal>
    </div>
  </UContainer>
</template>

<style scoped>
.custom-prose :deep(.ProseMirror) {
    padding: 0
}
</style>
