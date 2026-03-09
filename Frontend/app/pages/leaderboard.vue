<script setup lang="ts">
import { useIntersectionObserver } from '@vueuse/core'
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { PagingResult } from '~/apis/paging'
import Leaderboard from '~/components/home/leaderboard/Leaderboard.vue'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

export interface LeaderboardEntry {
  rank: number
  exp: number
  username: string
  pictureUrl: string
}

const PAGE_SIZE = 20

const pending = ref(false)
const initialLoading = ref(false)
const leaderboardData = ref<LeaderboardEntry[]>([])
const currentPage = ref(1)
const totalPages = ref(1)
const sentinel = ref<HTMLElement | null>(null)

const hasMore = computed(() => currentPage.value < totalPages.value)

async function fetchPage(page: number) {
  if (pending.value) return
  try {
    pending.value = true
    if (page === 1) initialLoading.value = true

    const response = await $authedFetch<PagingResult<LeaderboardEntry>>('/leaderboard', {
      query: { page, pageSize: PAGE_SIZE }
    })

    if (response.rows) {
      leaderboardData.value = page === 1 ? response.rows : [...leaderboardData.value, ...response.rows]
      totalPages.value = response.totalPages
      currentPage.value = page
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
    initialLoading.value = false
  }
}

onMounted(() => fetchPage(1))

useIntersectionObserver(sentinel, ([entry]) => {
  if (entry?.isIntersecting && hasMore.value && !pending.value) {
    fetchPage(currentPage.value + 1)
  }
})
</script>

<template>
  <UContainer>
    <div class="space-y-6">
      <UPageHeader
        title="Leaderboard"
        description="Top readers ranked by experience points"
      />

      <UCard>
        <div
          v-if="initialLoading"
          class="flex items-center justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <div
          v-else-if="leaderboardData.length === 0"
          class="flex flex-col items-center justify-center py-12"
        >
          <UIcon
            name="i-heroicons-trophy"
            class="size-16 text-gray-300 mb-4"
          />
          <p class="text-gray-500 text-center">
            No leaderboard data yet!<br>
            Start reading to be the first.
          </p>
        </div>

        <template v-else>
          <Leaderboard :data="leaderboardData" />

          <!-- sentinel for infinite scroll -->
          <div
            ref="sentinel"
            class="py-2 flex justify-center"
          >
            <UIcon
              v-if="pending"
              name="i-heroicons-arrow-path"
              class="animate-spin text-xl text-muted"
            />
          </div>
        </template>
      </UCard>
    </div>
  </UContainer>
</template>
