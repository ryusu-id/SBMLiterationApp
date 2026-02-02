<script setup lang="ts">
import { z } from 'zod'
import { $authedFetch, handleResponseError } from '~/apis/api'

export interface DailyReadFormState {
  id: number | null
  title: string
  coverImg: string
  content: string
  date: string
  category: string
  exp: number
  minimalCorrectAnswer: number
}

defineProps<{
  loading?: boolean
}>()

const action = ref<'Create' | 'Update'>('Create')

const state = reactive<DailyReadFormState>({
  id: null,
  title: '',
  coverImg: '',
  content: 'Write your daily read content here...',
  date: new Date().toISOString().split('T')[0] as string,
  category: '',
  exp: 0,
  minimalCorrectAnswer: 0
})

const schema = z.object({
  title: z.string().min(1, 'Title is required'),
  coverImg: z.string().optional(),
  content: z.string().min(1, 'Content is required'),
  date: z.string().min(1, 'Date is required'),
  category: z.string().min(1, 'Category is required'),
  exp: z.number().min(0, 'Experience points must be positive'),
  minimalCorrectAnswer: z.number().int().min(0, 'Minimal correct answer must be a positive integer')
})

type Schema = z.output<typeof schema>

const emit = defineEmits<{
  (e: 'submit', param: {
    action: 'Create' | 'Update'
    data: Omit<DailyReadFormState, 'id'>
    id: number | null
  }): void
}>()

const uploading = ref(false)
const renderKey = ref(0)
const toast = useToast()

async function handleFileUpload(files: File[]) {
  if (!files || !files[0] || files.length === 0) return

  const file = files[0]
  const formData = new FormData()
  formData.append('file', file)

  try {
    uploading.value = true
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
      state.coverImg = response.data.url
      toast.add({
        title: 'Image uploaded successfully',
        color: 'success'
      })
    }
  } catch (error) {
    handleResponseError(error)
  } finally {
    uploading.value = false
  }
}

function update(data: DailyReadFormState) {
  setState(data)
  action.value = 'Update'
}

function setState(data: Partial<DailyReadFormState>) {
  if (data.id !== undefined) state.id = data.id
  if (data.title !== undefined) state.title = data.title
  if (data.coverImg !== undefined) state.coverImg = data.coverImg
  if (data.content !== undefined) state.content = data.content
  if (data.date !== undefined) state.date = data.date
  if (data.category !== undefined) state.category = data.category
  if (data.exp !== undefined) state.exp = data.exp
  if (data.minimalCorrectAnswer !== undefined) state.minimalCorrectAnswer = data.minimalCorrectAnswer

  renderKey.value++
}

function resetState() {
  state.id = null
  state.title = ''
  state.coverImg = ''
  state.content = ''
  state.date = new Date().toISOString().split('T')[0] as string
  state.category = ''
  state.exp = 0
  state.minimalCorrectAnswer = 0
}

function onSubmit(event: { data: Schema }) {
  const data = event.data
  emit('submit', {
    action: action.value,
    data: {
      title: data.title,
      coverImg: data.coverImg || '',
      content: data.content,
      date: data.date,
      category: data.category,
      exp: data.exp,
      minimalCorrectAnswer: data.minimalCorrectAnswer
    },
    id: state.id
  })
}

defineExpose({
  update,
  setState,
  resetState
})
</script>

<template>
  <UForm
    :schema="schema"
    :state="state"
    class="space-y-6"
    @submit="onSubmit"
  >
    <UFormField
      label="Cover Image"
      name="coverImg"
    >
      <div class="space-y-3">
        <div class="grid grid-cols-1 md:grid-cols-[1fr_auto_1fr] gap-3 md:gap-4">
          <div class="space-y-2">
            <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
              Upload Image
            </label>
            <UInput
              type="file"
              accept="image/*"
              @change="(e) => handleFileUpload(Array.from((e.target as HTMLInputElement).files || []))"
            />
            <div
              v-if="uploading"
              class="text-sm text-gray-500"
            >
              Uploading...
            </div>
          </div>

          <div class="relative flex items-center gap-3 md:flex-col md:justify-center md:gap-0">
            <div class="flex-1 border-t border-gray-200 dark:border-gray-700 md:hidden" />
            <span class="text-xs text-gray-500 uppercase md:px-3 md:py-2">or</span>
            <div class="flex-1 border-t border-gray-200 dark:border-gray-700 md:hidden" />
          </div>

          <div class="space-y-2">
            <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
              Enter Image URL
            </label>
            <UInput
              v-model="state.coverImg"
              type="url"
              placeholder="https://example.com/image.jpg"
              class="w-full"
            />
          </div>
        </div>

        <div
          v-if="state.coverImg"
          class="space-y-2 pt-2"
        >
          <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
            Preview
          </label>
          <div class="flex items-start gap-3">
            <img
              :src="state.coverImg"
              alt="Cover image preview"
              class="w-32 aspect-[2/3] object-cover rounded border shadow-sm"
            >
            <div class="flex-1 min-w-0">
              <p class="text-xs text-gray-500 dark:text-gray-400 break-all font-mono bg-gray-50 dark:bg-gray-800 p-2 rounded">
                {{ state.coverImg }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </UFormField>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <UFormField
        label="Title"
        name="title"
        required
      >
        <UInput
          v-model="state.title"
          placeholder="Enter title"
          class="w-full"
        />
      </UFormField>

      <UFormField
        label="Date"
        name="date"
        required
      >
        <UInput
          v-model="state.date"
          type="date"
          class="w-full"
        />
      </UFormField>

      <CategorySelect
        v-model="state.category"
        name="category"
        required
      />

      <UFormField
        label="Experience Points"
        name="exp"
        required
      >
        <UInput
          v-model.number="state.exp"
          type="number"
          placeholder="Enter experience points"
          step="0.01"
          class="w-full"
        />
      </UFormField>

      <UFormField
        label="Minimal Correct Answer"
        name="minimalCorrectAnswer"
        required
        class="md:col-span-2"
      >
        <UInput
          v-model.number="state.minimalCorrectAnswer"
          type="number"
          placeholder="Enter minimal correct answer count"
          min="0"
          class="w-full"
        />
      </UFormField>
    </div>

    <UFormField
      label="Content"
      name="content"
      required
      class="col-span-full"
    >
      <MarkdownEditor
        v-model="state.content"
        :render-key="renderKey"
        placeholder="Write your daily read content here..."
      />
    </UFormField>

    <div class="flex justify-end gap-2 pt-4">
      <UButton
        type="submit"
        :loading="loading"
      >
        {{ action === 'Create' ? 'Create' : 'Update' }}
      </UButton>
    </div>
  </UForm>
</template>
