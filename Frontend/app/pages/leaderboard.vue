<script setup lang="ts">
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

const pending = ref(false)
const leaderboardData = ref<LeaderboardEntry[]>([])

onMounted(async () => {
  try {
    pending.value = true
    const response = await $authedFetch<PagingResult<LeaderboardEntry>>('/leaderboard', {
      query: {
        page: 1,
        pageSize: 100
      }
    })

    if (response.rows) {
      leaderboardData.value = response.rows
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
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
          v-if="pending"
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

        <Leaderboard
          v-else
          :data="leaderboardData"
        />
      </UCard>
    </div>
  </UContainer>
</template>
