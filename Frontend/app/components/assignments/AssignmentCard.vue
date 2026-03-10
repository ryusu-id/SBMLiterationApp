<script setup lang="ts">
import { FileAttachmentExtension } from '~/extensions/file-attachment'

export interface AssignmentItem {
  id: number
  title: string
  description?: string
  dueDate?: string
  submissionId: number
  isCompleted: boolean
  completedAt?: string
  fileCount: number
}

const props = defineProps<{
  assignment: AssignmentItem
  status: 'active' | 'missing' | 'done'
}>()

const badgeConfig = computed(() => {
  switch (props.status) {
    case 'active':
      return { label: 'Not Submitted', color: 'neutral' as const }
    case 'missing':
      return { label: 'Overdue', color: 'error' as const }
    case 'done':
      return { label: 'Submitted', color: 'success' as const }
    default:
      return { label: 'Unknown', color: 'neutral' as const }
  }
})

const cardClass = computed(() => {
  const base = 'transition-shadow cursor-pointer'
  switch (props.status) {
    case 'active':
      return `${base} hover:ring-1 hover:ring-primary`
    case 'missing':
      return `${base} hover:ring-1 hover:ring-error`
    case 'done':
      return `${base} hover:ring-1 hover:ring-primary opacity-75`
    default:
      return base
  }
})

function stripHtml(html: string): string {
  return html
    .replace(/<[^>]*>/g, ' ')
    .replace(/&nbsp;/g, ' ')
    .replace(/&amp;/g, '&')
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>')
    .replace(/&quot;/g, '"')
    .replace(/&#39;/g, '\'')
    .replace(/&[a-z]+;/gi, '')
    .replace(/\s+/g, ' ')
    .trim()
}
</script>

<template>
  <NuxtLink :to="`/assignments/${assignment.id}`">
    <UCard :class="cardClass">
      <div class="flex items-start justify-between gap-4">
        <div class="flex-1 min-w-0">
          <div class="flex items-start justify-between gap-2 min-w-0 overflow-hidden">
            <h3 class="text-lg font-semibold min-w-0 flex-1 mr-2">
              {{ assignment.title }}
            </h3>
            <UBadge
              :color="badgeConfig.color"
              variant="subtle"
              class="shrink-0"
            >
              {{ badgeConfig.label }}
            </UBadge>
          </div>
          <UEditor
            v-if="assignment.description"
            class="text-sm text-muted mt-1 line-clamp-2 custom-prose"
            :editable="false"
            content-type="markdown"
            :config="{ extensions: ['starter-kit', 'image'] }"
            :extensions="[FileAttachmentExtension]"
            :model-value="stripHtml(assignment.description)"
          />
          <div class="flex justify-between items-center gap-3 mt-2 flex-wrap">
            <span
              v-if="status !== 'done' && assignment.dueDate"
              class="text-xs text-muted flex items-center gap-2"
            >
              <UIcon
                name="i-lucide-calendar"
                class="size-3"
              />
              Due:
              {{
                new Date(assignment.dueDate).toLocaleString("en-GB", {
                  day: "numeric",
                  month: "short",
                  year: "numeric",
                  hour: "2-digit",
                  minute: "2-digit",
                  hour12: false
                })
              }}
            </span>
            <span
              v-if="status === 'done' && assignment.completedAt"
              class="text-xs text-muted flex items-center gap-2"
            >
              <UIcon
                name="i-lucide-check"
                class="size-3"
              />
              Submitted:
              {{
                new Date(assignment.completedAt).toLocaleString("en-GB", {
                  day: "numeric",
                  month: "short",
                  year: "numeric",
                  hour: "2-digit",
                  minute: "2-digit",
                  hour12: false
                })
              }}
            </span>
            <span class="text-xs text-muted flex items-center gap-1">
              <UIcon
                name="i-lucide-paperclip"
                class="size-3"
              />
              {{ assignment.fileCount || 0 }} file(s)
            </span>
          </div>
        </div>
      </div>
    </UCard>
  </NuxtLink>
</template>

<style scoped>
.custom-prose :deep(.ProseMirror) {
  padding: 0;
}

.custom-prose :deep(.ProseMirror h1),
.custom-prose :deep(.ProseMirror h2),
.custom-prose :deep(.ProseMirror h3),
.custom-prose :deep(.ProseMirror h4),
.custom-prose :deep(.ProseMirror h5),
.custom-prose :deep(.ProseMirror h6) {
  font-size: inherit;
  font-weight: inherit;
  line-height: inherit;
  margin: 0;
}
</style>
