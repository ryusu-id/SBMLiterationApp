<script setup lang="ts">
import { NodeViewWrapper } from '@tiptap/vue-3'
import type { NodeViewProps } from '@tiptap/vue-3'

// Accept all NodeViewProps so VueNodeViewRenderer is satisfied; we only use node & selected
const props = defineProps<NodeViewProps>()

function formatFileSize(bytes: number): string {
  if (!bytes) return ''
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function getFileIcon(contentType: string): string {
  if (!contentType) return 'i-lucide-file'
  if (contentType.includes('pdf')) return 'i-lucide-file-text'
  if (contentType.includes('word') || contentType.includes('document')) return 'i-lucide-file-text'
  if (contentType.includes('spreadsheet') || contentType.includes('excel') || contentType.includes('sheet')) return 'i-lucide-file-spreadsheet'
  if (contentType.includes('zip') || contentType.includes('rar') || contentType.includes('tar') || contentType.includes('compressed')) return 'i-lucide-file-archive'
  if (contentType.includes('video')) return 'i-lucide-file-video'
  if (contentType.includes('audio')) return 'i-lucide-file-audio'
  if (contentType.startsWith('text/')) return 'i-lucide-file-text'
  if (contentType.includes('json') || contentType.includes('xml')) return 'i-lucide-file-code'
  return 'i-lucide-file'
}

function getFileColorClass(contentType: string): string {
  if (!contentType) return 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400'
  if (contentType.includes('pdf')) return 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400'
  if (contentType.includes('word') || contentType.includes('document')) return 'bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400'
  if (contentType.includes('spreadsheet') || contentType.includes('excel') || contentType.includes('sheet')) return 'bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400'
  if (contentType.includes('zip') || contentType.includes('rar') || contentType.includes('tar')) return 'bg-yellow-100 dark:bg-yellow-900/30 text-yellow-600 dark:text-yellow-400'
  if (contentType.includes('video')) return 'bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400'
  if (contentType.includes('audio')) return 'bg-pink-100 dark:bg-pink-900/30 text-pink-600 dark:text-pink-400'
  return 'bg-primary-100 dark:bg-primary-900/30 text-primary-600 dark:text-primary-400'
}
</script>

<template>
  <NodeViewWrapper
    as="div"
    data-drag-handle
  >
    <div
      :class="[
        'my-2 flex items-center gap-3 p-3 rounded-lg border transition-all select-none cursor-default',
        props.selected
          ? 'border-primary ring-2 ring-primary/20 shadow-sm'
          : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600 hover:shadow-sm'
      ]"
      contenteditable="false"
    >
      <div
        :class="[
          'flex-shrink-0 flex items-center justify-center size-10 rounded-md',
          getFileColorClass(node.attrs.contentType)
        ]"
      >
        <UIcon
          :name="getFileIcon(node.attrs.contentType)"
          class="size-5"
        />
      </div>

      <div class="flex-1 min-w-0">
        <p class="text-sm font-medium text-gray-800 dark:text-gray-200 truncate">
          {{ node.attrs.fileName }}
        </p>
        <p
          v-if="node.attrs.fileSize"
          class="text-xs text-gray-500 dark:text-gray-400 mt-0.5"
        >
          {{ formatFileSize(node.attrs.fileSize) }}
        </p>
      </div>

      <a
        :href="node.attrs.src"
        target="_blank"
        rel="noopener noreferrer"
        class="flex-shrink-0 flex items-center gap-1.5 px-2.5 py-1.5 text-xs font-medium rounded-md bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
        @click.stop
      >
        <UIcon
          name="i-lucide-download"
          class="size-3.5"
        />
        Download
      </a>
    </div>
  </NodeViewWrapper>
</template>
