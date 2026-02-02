<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { EditorToolbarItem } from '@nuxt/ui'
import type { EditorView } from '@tiptap/pm/view'

const props = defineProps<{
  modelValue: string
  placeholder?: string
  renderKey?: number
}>()

const localRenderKey = ref(0)

const computedRenderKey = computed(() => {
  return props.renderKey !== undefined ? props.renderKey + localRenderKey.value : localRenderKey.value
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const content = computed({
  get: () => props.modelValue,
  set: value => emit('update:modelValue', value)
})

const contentImageUploading = ref(false)
const toast = useToast()
const fileInput = ref<HTMLInputElement>()
const editorView = ref<EditorView>()

async function uploadImage(file: File) {
  const formData = new FormData()
  formData.append('file', file)

  try {
    contentImageUploading.value = true
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
    }>('/files/upload', {
      method: 'POST',
      body: formData
    })

    if (response.data?.url) {
      return response.data.url
    }
    return null
  } catch (error) {
    handleResponseError(error)
    return null
  } finally {
    contentImageUploading.value = false
  }
}

async function handleFileInputChange(event: Event) {
  const target = event.target as HTMLInputElement
  const files = target.files
  if (!files || files.length === 0) return

  const file = files[0]
  if (!file)
    return
  const imageUrl = await uploadImage(file)

  if (imageUrl && editorView.value) {
    const { schema } = editorView.value.state
    const imageNode = schema.nodes.image?.create({ src: imageUrl })

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
  const items = event.clipboardData?.items
  if (!items) return false

  // Check for images first
  for (const item of Array.from(items)) {
    if (item.type.startsWith('image/')) {
      event.preventDefault()

      const file = item.getAsFile()
      if (!file) continue

      ;(async () => {
        const imageUrl = await uploadImage(file)

        if (imageUrl) {
          const { schema } = view.state
          const imageNode = schema.nodes.image?.create({ src: imageUrl })

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

  // Check for text/markdown content
  const textData = event.clipboardData?.getData('text/plain')
  if (textData) {
    // Increment render key to re-render the editor after paste
    nextTick(() => {
      localRenderKey.value++
    })
  }

  return false
}

function handleDrop(view: EditorView, event: DragEvent) {
  const files = event.dataTransfer?.files
  if (!files || files.length === 0) return false

  const imageFiles = Array.from(files).filter(file => file.type.startsWith('image/'))
  if (imageFiles.length === 0) return false

  event.preventDefault()

  imageFiles.forEach((file) => {
    ;(async () => {
      const imageUrl = await uploadImage(file)

      if (imageUrl) {
        const { schema } = view.state
        const imageNode = schema.nodes.image?.create({ src: imageUrl })

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
      :key="computedRenderKey"
      v-slot="{ editor }"
      v-model="content"
      :placeholder="placeholder || 'Start writing...'"
      :config="{
        extensions: ['starter-kit', 'image']
      }"
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
      v-if="contentImageUploading"
      class="absolute inset-0 flex items-center justify-center bg-black/10 rounded-md pointer-events-none"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-4 pointer-events-auto">
        <div class="flex items-center gap-3">
          <UIcon
            name="i-lucide-loader-circle"
            class="animate-spin text-2xl text-primary"
          />
          <span class="text-sm text-gray-600 dark:text-gray-400">Uploading image...</span>
        </div>
      </div>
    </div>
  </div>
</template>
