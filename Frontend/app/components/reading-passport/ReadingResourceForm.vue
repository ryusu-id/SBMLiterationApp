<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'
import { $authedFetch, handleResponseError } from '~/apis/api'
import GoogleBooksSearchModal from '~/components/recommendation/GoogleBooksSearchModal.vue'
import type { GoogleBooksSelection } from '~/components/recommendation/GoogleBooksSearchModal.vue'

interface JournalDoiResponse {
  message: string
  data: {
    title?: string[]
    author?: Array<{
      given?: string
      family?: string
      sequence?: string
      affiliation?: string[]
    }>
    issued?: {
      timestamp?: number
    }
    published?: {
      timestamp?: number
    }
    created: {
      timestamp: number
    }
    license?: Array<{
      url: string
      start: {
        timestamp: number
      }
      delayInDays: number
      contentVersion: string
    }>
    page?: string
    issn?: string[]
    issnType?: Array<{
      value: string
      type: string
    }>
    subject?: string[]
    doi?: string
    containerTitle?: string[]
  }
  errorCode?: string
  errorDescription?: string
  errors?: string[]
}

const props = defineProps<{
  loading?: boolean
  journal?: boolean
  isEdit?: boolean
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
  readingCategory: props.journal ? z.string().min(1, 'Category is required') : z.string().optional(),
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
  readingCategory: props.journal ? '' : undefined,
  page: NaN,
  resourceLink: '',
  coverImageUri: ''
})

const uploading = ref(false)
const doiFetching = ref(false)
const toast = useToast()
const googleBooksModal = useTemplateRef<typeof GoogleBooksSearchModal>('googleBooksModal')

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
  state.readingCategory = props.journal ? '' : undefined
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

function openGoogleBooksSearch() {
  googleBooksModal.value?.open()
}

function handleBookSelection(book: GoogleBooksSelection) {
  state.title = book.title
  state.isbn = book.isbn
  state.authors = book.authors ? book.authors.split(', ').filter(a => a.trim()) : ['']
  if (state.authors.length === 0) state.authors = ['']
  state.publishYear = book.publishYear
  state.readingCategory = book.category
  state.page = book.page
  state.resourceLink = book.resourceLink
  state.coverImageUri = book.coverImageUri

  toast.add({
    title: 'Book details imported from Google Books',
    color: 'success'
  })
}

async function handleDoiPaste(event: ClipboardEvent) {
  if (!props.journal) return

  let pastedText = event.clipboardData?.getData('text')
  if (!pastedText) return

  // Convert DOI identifier to full URL if needed
  pastedText = pastedText.trim()
  if (pastedText.match(/^10\.\d{4,}/)) {
    // It's a DOI identifier without URL, prepend the DOI resolver
    pastedText = `https://doi.org/${pastedText}`
  }

  try {
    doiFetching.value = true
    const response = await $authedFetch<JournalDoiResponse>('/journal-doi', {
      query: {
        doi: pastedText
      }
    })

    if (response.errorCode || response.errorDescription) {
      handleResponseError(response)
      return
    }

    if (response.data) {
      const data = response.data

      // Fill title
      if (data.title?.[0]) {
        state.title = data.title[0]
      }

      // Fill ISBN/ISSN
      if (data.issn?.[0]) {
        state.isbn = data.issn[0]
      } else if (data.issnType?.[0]?.value) {
        state.isbn = data.issnType[0].value
      }

      // Fill authors
      if (data.author && data.author.length > 0) {
        state.authors = data.author.map((author) => {
          const given = author.given || ''
          const family = author.family || ''
          return `${given} ${family}`.trim()
        }).filter(name => name.length > 0)
        if (state.authors.length === 0) state.authors = ['']
      }

      // Fill publish year
      const year
        = data.license && data.license.length > 0
          ? new Date(data.license[data.license.length - 1]?.start.timestamp || '').getFullYear()
          : data.issued?.timestamp
            ? new Date(data.issued.timestamp).getFullYear()
            : data.published?.timestamp
              ? new Date(data.published.timestamp).getFullYear()
              : data.created.timestamp
                ? new Date(data.created.timestamp).getFullYear()
                : null

      if (year) {
        state.publishYear = year.toString()
      }

      // Fill category from subject
      if (data.subject?.[0]) {
        state.readingCategory = data.subject[0]
      }

      // Fill page count - extract from page range if available
      if (data.page) {
        // Handle page ranges like "123-145" or just "123"
        const pageMatch = data.page.match(/(\d+)(?:-(\d+))?/)
        if (pageMatch) {
          const startPage = parseInt(pageMatch[1] || '0')
          const endPage = pageMatch[2] ? parseInt(pageMatch[2]) : startPage
          state.page = endPage - startPage + 1
        }
      }

      // resourceLink is already set by the paste event
      state.resourceLink = pastedText

      toast.add({
        title: 'Journal details imported from DOI',
        color: 'success'
      })
    }
  } catch (error) {
    handleResponseError(error)
  } finally {
    doiFetching.value = false
  }
}

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
      <!-- Google Books Search - only for books -->
      <div
        v-if="!journal"
        class="p-4 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700"
      >
        <div class="flex flex-col space-y-3 sm:space-y-0 sm:flex-row sm:items-center justify-between">
          <div>
            <p class="font-medium text-sm text-gray-900 dark:text-gray-100">
              Import from Google Books
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
              Search and autofill book details from Google Books API
            </p>
          </div>
          <UButton
            type="button"
            icon="i-heroicons-magnifying-glass"
            class="cursor-pointer w-full sm:w-fit justify-center"
            @click="openGoogleBooksSearch"
          >
            Search Books
          </UButton>
        </div>
      </div>

      <!-- Title field - full width -->
      <UFormField
        label="Title"
        name="title"
        required
      >
        <UInput
          v-model="state.title"
          :placeholder="journal ? 'Enter article title' : 'Enter book title'"
          size="lg"
          class="w-full"
        />
      </UFormField>

      <!-- ISBN and Publish Year - responsive grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
        <UFormField
          v-if="!journal"
          label="ISBN/Identifier"
          name="isbn"
          required
        >
          <UInput
            v-model="state.isbn"
            :disabled="isEdit"
            placeholder="Enter ISBN or other identifier"
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
            class="cursor-pointer"
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
            class="cursor-pointer"
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
        <CategorySelect
          v-model="state.readingCategory"
          name="readingCategory"
          size="lg"
          :required="!journal"
        />

        <UFormField
          label="Page Count"
          name="page"
          :required="!journal"
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
        <div class="relative">
          <UInput
            v-model="state.resourceLink"
            type="url"
            :placeholder="journal ? 'Paste DOI link to auto-fill form' : 'https://example.com/resource'"
            size="lg"
            class="w-full"
            :disabled="doiFetching || isEdit"
            @paste="journal ? handleDoiPaste($event) : undefined"
          />
          <div
            v-if="doiFetching"
            class="absolute right-3 top-1/2 -translate-y-1/2"
          >
            <UIcon
              name="i-heroicons-arrow-path"
              class="animate-spin text-primary"
            />
          </div>
        </div>
        <template
          v-if="journal"
          #help
        >
          <p class="text-xs text-gray-500">
            Paste a DOI link to automatically fetch and fill journal details
          </p>
        </template>
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
          class="px-8 w-full text-center flex justify-center cursor-pointer"
          :loading
        >
          Save
        </UButton>
      </div>
    </UForm>
  </UCard>

  <GoogleBooksSearchModal
    ref="googleBooksModal"
    @select="handleBookSelection"
  />
</template>
