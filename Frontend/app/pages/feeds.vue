<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { PagingResult } from '~/apis/paging'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

export interface ActivityFeedItem {
  user: {
    nim: string
    fullname: string
    programStudy: string
    faculty: string
    generationYear: string
    pictureUrl: string
  }
  activityType: string
  activityDate: string
  description: string
}

const feeds = ref<ActivityFeedItem[]>([])
const loading = ref(false)
const loadingMore = ref(false)
const page = ref(1)
const totalPages = ref(0)
const hasMore = computed(() => page.value < totalPages.value)

async function fetchFeeds(pageNum: number, append = false) {
  try {
    if (append) {
      loadingMore.value = true
    } else {
      loading.value = true
    }

    const response = await $authedFetch<PagingResult<ActivityFeedItem>>('/feeds', {
      query: {
        page: pageNum,
        rowsPerPage: 15
      }
    })

    if (response.rows && Array.isArray(response.rows)) {
      if (append) {
        feeds.value = [...feeds.value, ...response.rows]
      } else {
        feeds.value = response.rows
      }
      totalPages.value = response.totalPages
      page.value = pageNum
    }
  } catch (error) {
    handleResponseError(error)
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

function handleScroll() {
  const scrollHeight = document.documentElement.scrollHeight
  const scrollTop = document.documentElement.scrollTop
  const clientHeight = document.documentElement.clientHeight

  if (scrollTop + clientHeight >= scrollHeight - 100 && hasMore.value && !loadingMore.value) {
    fetchFeeds(page.value + 1, true)
  }
}

onMounted(() => {
  fetchFeeds(1)
  window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})

function formatDate(dateString: string) {
  const date = new Date(dateString)
  const now = new Date()
  const diffInMs = now.getTime() - date.getTime()
  const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60))
  const diffInDays = Math.floor(diffInHours / 24)

  if (diffInHours < 1) {
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60))
    return diffInMinutes < 1 ? 'Just now' : `${diffInMinutes}m ago`
  } else if (diffInHours < 24) {
    return `${diffInHours}h ago`
  } else if (diffInDays < 7) {
    return `${diffInDays}d ago`
  } else {
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
  }
}

function getActivityIcon(activityType: string) {
  const iconMap: Record<string, string> = {
    DailyReadsExp: 'i-heroicons-calendar',
    StreakExp: 'i-heroicons-fire',
    BookComplete: 'i-heroicons-book-open',
    JournalComplete: 'i-heroicons-document-text'
  }
  return iconMap[activityType] || 'i-heroicons-star'
}
</script>

<template>
  <UContainer>
    <div class="flex flex-col space-y-8">
      <UPageHeader
        class="border-none"
      >
        <template #title>
          <h1 class="text-[36px] font-extrabold tracking-tighter">
            Activity Feeds
          </h1>
        </template>

        <template #description>
          <p class="tracking-tight text-[20px]">
            Recent activities from the community
          </p>
        </template>
      </UPageHeader>

      <div
        v-if="loading"
        class="flex items-center justify-center py-12"
      >
        <UIcon
          name="i-heroicons-arrow-path"
          class="animate-spin text-4xl"
        />
      </div>

      <div
        v-else-if="feeds.length === 0"
        class="flex flex-col items-center justify-center py-12"
      >
        <UIcon
          name="i-heroicons-rss"
          class="size-16 text-gray-300 mb-4"
        />
        <p class="text-gray-500 text-center">
          No activities yet!
        </p>
      </div>

      <div
        v-else
        class="space-y-0 divide-y divide-gray-200 dark:divide-gray-800"
      >
        <div
          v-for="(feed, index) in feeds"
          :key="`${feed.activityDate}-${index}`"
          class="flex items-center gap-4 py-4 px-2 hover:bg-gray-50 dark:hover:bg-gray-900/50 transition-colors"
        >
          <div class="relative">
            <UAvatar
              :src="feed.user.pictureUrl"
              :alt="feed.user.fullname"
              size="3xl"
            />
            <div class="absolute -bottom-1 -right-1 bg-primary w-[24px] h-[24px] rounded-full flex items-center justify-center">
              <UIcon
                :name="getActivityIcon(feed.activityType)"
                class="size-4 text-white"
              />
            </div>
          </div>

          <div class="flex-1 min-w-0">
            <div class="flex items-baseline gap-1.5">
              <p class="font-semibold text-gray-900 dark:text-white text-base">
                {{ feed.user.fullname }}
              </p>
            </div>
            <p class="text-gray-600 dark:text-gray-400 text-sm mt-0.5">
              <span class="font-medium text-gray-900 dark:text-white ml-1">{{ feed.description }}</span>
            </p>
          </div>

          <div class="text-gray-500 dark:text-gray-500 text-sm flex-shrink-0">
            {{ formatDate(feed.activityDate) }}
          </div>
        </div>

        <div
          v-if="loadingMore"
          class="flex items-center justify-center py-8"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-2xl text-gray-400"
          />
        </div>

        <div
          v-else-if="!hasMore && feeds.length > 0"
          class="flex items-center justify-center py-8"
        >
          <p class="text-gray-400 text-sm">
            No more activities to load
          </p>
        </div>
      </div>
    </div>
  </UContainer>
</template>
