<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'
import { $authedFetch, handleResponseError } from '~/apis/api'

const props = defineProps<{
  loading?: boolean
  journal?: boolean
}>()

const schema = z.object({
  title: z.string().min(1, 'Title is required'),
  isbn: props.journal
    ? z.string().optional()
    : z.string().min(1, 'ISBN is required'),
  authors: z
    .array(z.string().min(1, 'Author name is required'))
    .min(1, 'At least one author is required'),
  publishYear: z
    .string()
    .regex(/^\d{4}$/, 'Publish year must be a 4-digit year'),
  readingCategory: z.string().min(1, 'Category is required'),
  page: z.coerce.number().min(1, 'Page count must be at least 1'),
  resourceLink: z.url('Must be a valid URL').optional().or(z.literal('')),
  coverImageUri: z.url().optional().or(z.literal(''))
})

export type ReadingResourceSchema = z.output<typeof schema>

const state = reactive({
  title: '',
  isbn: '',
  authors: [''],
  publishYear: '',
  readingCategory: '',
  page: NaN,
  resourceLink: '',
  coverImageUri: ''
})

const uploading = ref(false)
const toast = useToast()

const emit = defineEmits<{
  (
    e: 'submit',
    data: Omit<ReadingResourceSchema, 'authors'> & { authors: string },
  ): void
}>()

function addAuthor() {
  state.authors.push('')
}

function removeAuthor(index: number) {
  if (state.authors.length > 1) {
    state.authors.splice(index, 1)
  }
}

function setState(data: Partial<ReadingResourceSchema>) {
  if (data.title !== undefined) state.title = data.title
  if (data.isbn !== undefined) state.isbn = data.isbn
  if (data.authors !== undefined)
    state.authors = data.authors.length > 0 ? data.authors : ['']
  if (data.publishYear !== undefined) state.publishYear = data.publishYear
  if (data.readingCategory !== undefined) state.readingCategory = data.readingCategory
  if (data.page !== undefined) state.page = data.page
  if (data.resourceLink !== undefined) state.resourceLink = data.resourceLink
  if (data.coverImageUri !== undefined) state.coverImageUri = data.coverImageUri
}

function resetState() {
  state.title = ''
  state.isbn = ''
  state.authors = ['']
  state.publishYear = ''
  state.readingCategory = ''
  state.page = NaN
  state.resourceLink = ''
  state.coverImageUri = ''
}

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
      state.coverImageUri = response.data.url
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

defineExpose({
  setState,
  resetState
})

async function onSubmit(event: FormSubmitEvent<ReadingResourceSchema>) {
  emit('submit', {
    ...event.data,
    authors: event.data.authors
      .map(author => author.trim())
      .filter(author => author.length > 0)
      .join(', ')
  })
  // TODO: loading state must be implemented, maybe from props
}
</script>

<template>
  <UCard
    variant="soft"
    :ui="{
      body: 'bg-white dark:bg-transparent'
    }"
  >
    <UForm
      :schema="schema"
      :state="state"
      class="space-y-6"
      @submit="onSubmit"
    >
      <!-- Title field - full width -->
      <UFormField
        label="Title"
        name="title"
        required
      >
        <UInput
          v-model="state.title"
          placeholder="Enter book title"
          size="lg"
          class="w-full"
        />
      </UFormField>

      <!-- ISBN and Publish Year - responsive grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
        <UFormField
          v-if="!journal"
          label="ISBN"
          name="isbn"
          required
        >
          <UInput
            v-model="state.isbn"
            placeholder="Enter ISBN"
            size="lg"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Publish Year"
          name="publishYear"
          required
        >
          <UInput
            v-model="state.publishYear"
            type="text"
            placeholder="Enter publish year"
            size="lg"
            maxlength="4"
            class="w-full"
          />
        </UFormField>
      </div>

      <!-- Authors field - dynamic array -->
      <div class="space-y-3">
        <div class="flex items-center justify-between">
          <label
            class="block text-sm font-medium text-gray-700 dark:text-gray-200"
          >
            Authors <span class="text-red-500">*</span>
          </label>
          <UButton
            type="button"
            color="primary"
            variant="soft"
            icon="i-heroicons-plus"
            @click="addAuthor"
          >
            Add Author
          </UButton>
        </div>

        <div
          v-for="(author, index) in state.authors"
          :key="index"
          class="flex gap-2 items-start"
        >
          <div class="flex-1">
            <UFormField
              :name="`authors[${index}]`"
              required
            >
              <UInput
                v-model="state.authors[index]"
                :placeholder="`Author ${index + 1} name`"
                size="lg"
                class="w-full"
              />
            </UFormField>
          </div>
          <UButton
            v-if="state.authors.length > 1"
            type="button"
            size="lg"
            color="error"
            variant="soft"
            icon="i-heroicons-trash"
            square
            @click="removeAuthor(index)"
          />
        </div>
        <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
          Add multiple authors as needed. At least one author is required.
        </p>
      </div>

      <!-- Book Category and Page Count - responsive grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
        <UFormField
          label="Book Category"
          name="bookCategory"
          required
        >
          <UInput
            v-model="state.readingCategory"
            placeholder="Enter book category"
            size="lg"
            class="w-full"
          />
        </UFormField>

        <UFormField
          label="Page Count"
          name="page"
          required
        >
          <UInput
            v-model="state.page"
            type="number"
            placeholder="Enter page count"
            size="lg"
            class="w-full"
          />
        </UFormField>
      </div>

      <!-- Resource Link - full width -->
      <UFormField
        :label="journal ? 'DOI Link' : 'Google Books Link'"
        name="resourceLink"
      >
        <UInput
          v-model="state.resourceLink"
          type="url"
          placeholder="https://example.com/resource"
          size="lg"
          class="w-full"
        />
      </UFormField>

      <!-- Cover Image - full width -->
      <UFormField
        label="Cover Image"
        name="coverImageUri"
      >
        <div class="space-y-3">
          <div class="space-y-2">
            <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
              Upload Image
            </label>
            <UInput
              type="file"
              accept="image/*"
              size="lg"
              @change="(e) => handleFileUpload(Array.from((e.target as HTMLInputElement).files || []))"
            />
            <div
              v-if="uploading"
              class="text-sm text-gray-500"
            >
              Uploading...
            </div>
          </div>

          <div class="relative flex items-center gap-3">
            <div class="flex-1 border-t border-gray-200 dark:border-gray-700" />
            <span class="text-xs text-gray-500 uppercase">or</span>
            <div class="flex-1 border-t border-gray-200 dark:border-gray-700" />
          </div>

          <div class="space-y-2">
            <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
              Enter Image URL
            </label>
            <UInput
              v-model="state.coverImageUri"
              type="url"
              placeholder="https://example.com/image.jpg"
              size="lg"
              class="w-full"
            />
          </div>

          <div
            v-if="state.coverImageUri"
            class="space-y-2 pt-2"
          >
            <label class="text-sm font-medium text-gray-700 dark:text-gray-200">
              Preview
            </label>
            <div class="flex items-start gap-3">
              <img
                :src="state.coverImageUri"
                alt="Book cover preview"
                class="w-32 aspect-[2/3] object-cover rounded border shadow-sm"
              >
              <div class="flex-1 min-w-0">
                <p class="text-xs text-gray-500 dark:text-gray-400 break-all font-mono bg-gray-50 dark:bg-gray-800 p-2 rounded">
                  {{ state.coverImageUri }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </UFormField>

      <!-- Submit button -->
      <div class="flex justify-end pt-4">
        <UButton
          type="submit"
          size="lg"
          class="px-8 w-full text-center flex justify-center"
          :loading
        >
          Save
        </UButton>
      </div>
    </UForm>
  </UCard>
</template>
