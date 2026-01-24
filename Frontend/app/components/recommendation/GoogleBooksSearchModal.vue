<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { PagingResult } from '~/apis/paging'
import ReadingRecomendation from '../home/Recomendation/ReadingRecomendation.vue'

export interface GoogleBookVolume {
  kind: string
  id: string
  etag: string
  selfLink: string
  volumeInfo: {
    title: string
    subtitle?: string
    authors?: string[]
    publisher?: string
    publishedDate?: string
    description?: string
    industryIdentifiers?: Array<{
      type: string
      identifier: string
    }>
    readingModes?: {
      text: boolean
      image: boolean
    }
    pageCount?: number
    printType?: string
    categories?: string[]
    maturityRating?: string
    allowAnonLogging?: boolean
    contentVersion?: string
    panelizationSummary?: {
      containsEpubBubbles: boolean
      containsImageBubbles: boolean
    }
    imageLinks?: {
      smallThumbnail?: string
      thumbnail?: string
    }
    language?: string
    previewLink?: string
    infoLink?: string
    canonicalVolumeLink?: string
  }
}

const emit = defineEmits<{
  (e: 'select', book: GoogleBookVolume): void
}>()

const open = ref(false)
const searchQuery = ref('')
const results = ref<GoogleBookVolume[]>([])
const pending = ref(false)
const hasSearched = ref(false)

function openModal() {
  open.value = true
  searchQuery.value = ''
  results.value = []
  hasSearched.value = false
}

function close() {
  open.value = false
}

defineExpose({
  open: openModal,
  close
})

async function search() {
  if (!searchQuery.value.trim()) return

  try {
    pending.value = true
    hasSearched.value = true
    const response = await $authedFetch<PagingResult<GoogleBookVolume>>('/google-books/search', {
      query: {
        query: searchQuery.value
      }
    })

    if (response.rows) {
      results.value = response.rows
    } else {
      handleResponseError(response)
    }
  } catch (error) {
    handleResponseError(error)
  } finally {
    pending.value = false
  }
}

function selectBook(book: GoogleBookVolume) {
  emit('select', book)
  close()
}

function mapToRecommendationCard(book: GoogleBookVolume) {
  const isbn = book.volumeInfo.industryIdentifiers?.find(id => id.type === 'ISBN_13' || id.type === 'ISBN_10')?.identifier || ''

  return {
    id: 0,
    title: book.volumeInfo.title || 'Untitled',
    author: book.volumeInfo.authors?.join(', ') || 'Unknown Author',
    imageUrl: book.volumeInfo.imageLinks?.thumbnail || book.volumeInfo.imageLinks?.smallThumbnail || '',
    totalPage: book.volumeInfo.pageCount || 0,
    category: book.volumeInfo.categories?.[0] || 'General',
    isbn,
    xp: 0
  }
}
</script>

<template>
  <UModal
    v-model:open="open"
    title="Search Google Books"
    description="Search for books from Google Books API to autofill the form"
    :ui="{
      content: 'max-w-screen'
    }"
  >
    <template #body>
      <div class="space-y-6">
        <UForm
          class="flex gap-2"
          @submit.prevent="search"
        >
          <UInput
            v-model="searchQuery"
            placeholder="Enter book title, author, or ISBN..."
            class="flex-1"
            size="lg"
          />
          <UButton
            type="submit"
            :loading="pending"
            size="lg"
          >
            Search
          </UButton>
        </UForm>

        <div
          v-if="pending"
          class="flex items-center justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <div
          v-else-if="hasSearched && results.length === 0"
          class="flex flex-col items-center justify-center py-12"
        >
          <UIcon
            name="i-heroicons-book-open"
            class="size-16 text-gray-300 mb-4"
          />
          <p class="text-gray-500 text-center">
            No books found. Try a different search query.
          </p>
        </div>

        <div
          v-else-if="results.length > 0"
          class="space-y-4"
        >
          <p class="text-sm text-gray-600">
            Found {{ results.length }} books. Click on a book to select it.
          </p>
          <div class="flex flex-wrap justify-evenly gap-4 overflow-y-auto pr-2">
            <button
              v-for="book in results"
              :key="book.id"
              type="button"
              class="transition-transform focus:outline-none focus:ring-2 focus:ring-primary rounded-[22px] max-w-60"
              @click="selectBook(book)"
            >
              <ReadingRecomendation
                google-books
                :book="mapToRecommendationCard(book)"
              />
            </button>
          </div>
        </div>

        <div
          v-if="!hasSearched"
          class="flex flex-col items-center justify-center py-12 text-gray-400"
        >
          <UIcon
            name="i-heroicons-magnifying-glass"
            class="size-16 mb-4"
          />
          <p class="text-center">
            Enter a search query to find books
          </p>
        </div>
      </div>
    </template>
  </UModal>
</template>
