<script setup lang="ts">
import { handleResponseError } from '~/apis/api'
import type { PagingResult } from '~/apis/paging'
import ReadingRecomendation from './ReadingRecomendation.vue'
import { Swiper, SwiperSlide } from 'swiper/vue'

// Import Swiper styles
import 'swiper/css'
import 'swiper/css/pagination'

import { Mousewheel, Pagination } from 'swiper/modules'

export interface ReadingRecommendation {
  id: number
  title: string
  isbn: string
  readingCategory: string
  authors: string
  publishYear: string
  page: number
  resourceLink?: string
  coverImageUri: string
}

const { $useAuthedFetch } = useNuxtApp()
const refreshCallback = ref<() => void>()

defineExpose({
  fetch: refreshCallback
})

const { data: response, pending: loading, error, refresh } = await $useAuthedFetch<PagingResult<ReadingRecommendation>>('/reading-recommendations/participant', {
  query: {
    page: 1,
    pageSize: 20
  }
})

refreshCallback.value = refresh

watch(error, (err) => {
  if (err) handleResponseError(err)
})

const books = computed(() => response.value?.rows || [])

const modules = [Pagination, Mousewheel]

const emit = defineEmits<{
  (e: 'refresh'): void
}>()
function handleRefresh() {
  refresh()
  emit('refresh')
}
</script>

<template>
  <div>
    <div>
      <h1 class="font-semibold text-[20px] tracking-tight">
        Don't know what to read?
      </h1>
      <h6 class="text-[16px] tracking-tight mb-[20px]">
        We have recommendations
      </h6>
    </div>

    <div
      v-if="loading"
      class="flex items-center justify-center py-12"
    >
      <UIcon
        name="i-heroicons-arrow-path"
        class="animate-spin text-4xl"
      />
    </div>

    <!-- Swiper Carousel -->
    <swiper
      v-else-if="books.length > 0"
      :space-between="12"
      :pagination="{ clickable: true }"
      :modules="modules"
      :slides-per-view="'auto'"
      mousewheel
      class="mb-8"
    >
      <swiper-slide
        v-for="book in books"
        :key="book.id"
        class="min-w-fit"
      >
        <ReadingRecomendation
          :book="{
            id: book.id,
            title: book.title,
            imageUrl: book.coverImageUri,
            category: book.readingCategory,
            author: book.authors,
            totalPage: book.page,
            xp: 30
          }"
          @refresh="handleRefresh"
        />
      </swiper-slide>
    </swiper>

    <div
      v-else
      class="flex flex-col items-center justify-center py-12"
    >
      <UIcon
        name="i-heroicons-book-open"
        class="size-16 text-gray-300 mb-4"
      />
      <p class="text-gray-500 text-center">
        No recommendations available at the moment
      </p>
    </div>

    <!-- List Bawah -->
    <!-- <div class="flex flex-col gap-y-4">
      <ReadingRecomendation
        v-for="(book, index) in books"
        :key="index"
        :book="book"
      />
    </div> -->
  </div>
</template>
