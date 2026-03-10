<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { EditorToolbarItem } from '@nuxt/ui'
import type { EditorView } from '@tiptap/pm/view'
import type { Editor as TiptapEditor } from '@tiptap/core'
import type { ShallowRef } from 'vue'
import { FileAttachmentExtension } from '~/extensions/file-attachment'

interface UploadResult {
  url: string
  fileName: string
  fileSize: number
  contentType: string
}

const props = defineProps<{
  modelValue: string
  placeholder?: string
  renderKey?: number
}>()

const computedRenderKey = computed(() => props.renderKey ?? 0)

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const content = computed({
  get: () => props.modelValue,
  set: value => emit('update:modelValue', value)
})

const contentImageUploading = ref(false)
const contentFileUploading = ref(false)
const toast = useToast()
const fileInput = ref<HTMLInputElement>()
const editorView = ref<EditorView>()

// Template ref to access the underlying TipTap editor instance
interface UEditorInstance {
  editor: ShallowRef<TiptapEditor | undefined>
}
const uEditorRef = useTemplateRef<UEditorInstance>('uEditorRef')

async function uploadFile(file: File, isImage = false): Promise<UploadResult | null> {
  const formData = new FormData()
  formData.append('file', file)

  try {
    if (isImage) contentImageUploading.value = true
    else contentFileUploading.value = true

    const response = await $authedFetch<{
      message: string
      data: {
        url: string
        fileName: string
        fileSize: number
        contentType: string
      }
      errorCode?: string
      errorDescription?: string
      errors?: string[]
    }>(isImage ? '/files/upload' : '/files/assignment/upload', {
      method: 'POST',
      body: formData
    })

    if (response.data?.url) {
      return {
        url: response.data.url,
        fileName: response.data.fileName || file.name,
        fileSize: response.data.fileSize || file.size,
        contentType: response.data.contentType || file.type
      }
    }
    return null
  } catch (error) {
    handleResponseError(error)
    return null
  } finally {
    if (isImage) contentImageUploading.value = false
    else contentFileUploading.value = false
  }
}

async function handleFileInputChange(event: Event) {
  const target = event.target as HTMLInputElement
  const files = target.files
  if (!files || files.length === 0) return

  const file = files[0]
  if (!file) return

  const result = await uploadFile(file, true)

  if (result && editorView.value) {
    const { schema } = editorView.value.state
    const imageNode = schema.nodes.image?.create({ src: result.url })

    if (imageNode) {
      const transaction = editorView.value.state.tr.replaceSelectionWith(imageNode)
      editorView.value.dispatch(transaction)

      toast.add({
        title: 'Image uploaded and inserted',
        color: 'success'
      })
    }
  }

  // Reset input
  target.value = ''
}

function handleContentImagePaste(view: EditorView, event: ClipboardEvent) {
  editorView.value = view
  const items = event.clipboardData?.items
  if (!items) return false

  // Check for images first
  for (const item of Array.from(items)) {
    if (item.type.startsWith('image/')) {
      event.preventDefault()

      const file = item.getAsFile()
      if (!file) continue

      ;(async () => {
        const result = await uploadFile(file, true)

        if (result) {
          const { schema } = view.state
          const imageNode = schema.nodes.image?.create({ src: result.url })

          if (imageNode) {
            const transaction = view.state.tr.replaceSelectionWith(imageNode)
            view.dispatch(transaction)

            toast.add({
              title: 'Image uploaded and inserted',
              color: 'success'
            })
          }
        }
      })()

      return true
    }
  }

  // Text/markdown paste is handled natively by TipTap's markdown extension.
  return false
}

function handleDrop(view: EditorView, event: DragEvent) {
  editorView.value = view
  const files = event.dataTransfer?.files
  if (!files || files.length === 0) return false

  const imageFiles = Array.from(files).filter(file => file.type.startsWith('image/'))
  const attachmentFiles = Array.from(files).filter(file => !file.type.startsWith('image/'))

  if (imageFiles.length === 0 && attachmentFiles.length === 0) return false

  event.preventDefault()

  // Handle image files
  imageFiles.forEach((file) => {
    ;(async () => {
      const result = await uploadFile(file, true)

      if (result) {
        const { schema } = view.state
        const imageNode = schema.nodes.image?.create({ src: result.url })

        if (imageNode) {
          const transaction = view.state.tr.replaceSelectionWith(imageNode)
          view.dispatch(transaction)

          toast.add({
            title: 'Image uploaded and inserted',
            color: 'success'
          })
        }
      }
    })()
  })

  // Handle non-image file attachments
  attachmentFiles.forEach((file) => {
    ;(async () => {
      const result = await uploadFile(file, false)

      if (result) {
        const { schema } = view.state
        const nodeType = schema.nodes.fileAttachment
        console.log(schema.nodes)
        if (nodeType) {
          const node = nodeType.create({
            src: result.url,
            fileName: result.fileName,
            fileSize: result.fileSize,
            contentType: result.contentType
          })
          const transaction = view.state.tr.replaceSelectionWith(node)
          view.dispatch(transaction)
          toast.add({ title: `"${result.fileName}" attached`, color: 'success' })
        }
      }
    })()
  })

  return true
}

const toolbarItems: EditorToolbarItem[][] = [
  [
    {
      icon: 'i-lucide-heading',
      tooltip: { text: 'Headings' },
      content: {
        align: 'start'
      },
      items: [
        {
          kind: 'heading',
          level: 1,
          icon: 'i-lucide-heading-1',
          label: 'Heading 1'
        },
        {
          kind: 'heading',
          level: 2,
          icon: 'i-lucide-heading-2',
          label: 'Heading 2'
        },
        {
          kind: 'heading',
          level: 3,
          icon: 'i-lucide-heading-3',
          label: 'Heading 3'
        }
      ]
    }
  ],
  [
    {
      kind: 'mark',
      mark: 'bold',
      icon: 'i-lucide-bold',
      tooltip: { text: 'Bold' }
    },
    {
      kind: 'mark',
      mark: 'italic',
      icon: 'i-lucide-italic',
      tooltip: { text: 'Italic' }
    },
    {
      kind: 'mark',
      mark: 'underline',
      icon: 'i-lucide-underline',
      tooltip: { text: 'Underline' }
    },
    {
      kind: 'mark',
      mark: 'strike',
      icon: 'i-lucide-strikethrough',
      tooltip: { text: 'Strikethrough' }
    },
    {
      kind: 'mark',
      mark: 'code',
      icon: 'i-lucide-code',
      tooltip: { text: 'Code' }
    }
  ],
  [
    {
      kind: 'bulletList',
      icon: 'i-lucide-list',
      tooltip: { text: 'Bullet List' }
    },
    {
      kind: 'orderedList',
      icon: 'i-lucide-list-ordered',
      tooltip: { text: 'Numbered List' }
    }
  ],
  [
    {
      kind: 'blockquote',
      icon: 'i-lucide-quote',
      tooltip: { text: 'Blockquote' }
    },
    {
      kind: 'codeBlock',
      icon: 'i-lucide-square-code',
      tooltip: { text: 'Code Block' }
    },
    {
      kind: 'link',
      icon: 'i-lucide-link',
      tooltip: { text: 'Link' }
    }
  ]
]
</script>

<template>
  <div class="relative">
    <input
      ref="fileInput"
      type="file"
      accept="image/*"
      class="hidden"
      @change="handleFileInputChange"
    >

    <UEditor
      ref="uEditorRef"
      :key="computedRenderKey"
      v-slot="{ editor }"
      v-model="content"
      :placeholder="placeholder || 'Start writing...'"
      :config="{
        extensions: ['starter-kit', 'image']
      }"
      :extensions="[FileAttachmentExtension]"
      class="w-full min-h-64 border border-gray-300 dark:border-gray-700 rounded-md"
      :ui="{
        content: 'py-4'
      }"
      :editor-props="{
        handlePaste: handleContentImagePaste,
        handleDrop: handleDrop
      }"
      editable
      content-type="markdown"
    >
      <UEditorToolbar
        :editor="editor"
        :items="toolbarItems"
        layout="fixed"
        class="border-b border-gray-200 dark:border-gray-700"
      />
      <UEditorDragHandle :editor="editor" />
    </UEditor>
    <div
      v-if="contentImageUploading || contentFileUploading"
      class="absolute inset-0 flex items-center justify-center bg-black/10 rounded-md pointer-events-none"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-4 pointer-events-auto">
        <div class="flex items-center gap-3">
          <UIcon
            name="i-lucide-loader-circle"
            class="animate-spin text-2xl text-primary"
          />
          <span class="text-sm text-gray-600 dark:text-gray-400">
            {{ contentImageUploading ? 'Uploading image...' : 'Uploading attachment...' }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>
