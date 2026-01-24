<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { PagingResult } from '~/apis/paging'
import ReadingRecomendationList from '~/components/home/Recomendation/ReadingRecomendationList.vue'
import ReadingReportList from '~/components/reading-passport/ReadingReportList.vue'
import type { ReadingReportData } from '~/components/reading-passport/ReadingReportList.vue'
import ReadingResources from '~/components/reading-passport/ReadingResources.vue'
import Streak from '~/components/reading-passport/Streak.vue'

// definePageMeta({
//   middleware: 'auth'
// })

definePageMeta({
  keepalive: true
})

const getCurrentWeekDates = () => {
  const week = []
  const today = new Date()
  const currentDay = today.getDay()
  const startOfWeek = new Date(today)
  startOfWeek.setDate(today.getDate() - currentDay)

  for (let i = 0; i < 7; i++) {
    const date = new Date(startOfWeek)
    date.setDate(startOfWeek.getDate() + i)
    week.push({
      date: date,
      day: date.toLocaleDateString('en-US', { weekday: 'long' }),
      isStreak: Math.random() > 0.5 // Replace with actual streak logic
    })
  }
  return week
}

const weekDates = ref<
  {
    date: Date
    day: string
    isStreak: boolean
  }[]
>([])

const booksRef = useTemplateRef<typeof ReadingResources>('books')
const journalsRef = useTemplateRef<typeof ReadingResources>('journals')
const recommendation = useTemplateRef<typeof ReadingRecomendationList>('recommendation')

const readingReports = ref<ReadingReportData[]>([])
const reportPending = ref(false)
async function fetchReport() {
  try {
    reportPending.value = true
    const response = await $authedFetch<PagingResult<ReadingReportData>>('/reading-resources/reports', {
      query: {
        page: 1,
        pageSize: 100
      }
    })
    if (response.rows) {
      readingReports.value = response.rows
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    reportPending.value = false
  }
}

function fetchReadingResources() {
  booksRef.value?.fetch()
  journalsRef.value?.fetch()
}

function fetchRecommendation() {
  recommendation.value?.fetch()
}

onMounted(async () => {
  weekDates.value = getCurrentWeekDates()

  await fetchReport()
})

onUpdated(async () => {
  await fetchReport()
})

const tabs = [
  {
    label: 'Books',
    icon: 'i-lucide-book',
    slot: 'books'
  },
  {
    label: 'Research Article',
    icon: 'i-lucide-form',
    slot: 'journal-paper'
  }
]
</script>

<template>
  <UContainer
    class="max-w-[950px]"
  >
    <div class="flex flex-col space-y-[32px]">
      <div class="flex flex-col md:flex-row mb-0">
        <UPageHeader
          class="border-none"
          :ui="{
            root: 'w-full'
          }"
        >
          <template #title>
            <div class="flex flex-col md:flex-row md:space-x-2">
              <h1 class="text-[36px] font-extrabold tracking-tighter">
                Welcome,
              </h1>
              <h1
                class="text-[36px] font-extrabold tracking-tighter leading-none"
              >
                Budi
              </h1>
            </div>
          </template>

          <template #description>
            <p class="tracking-tight text-[20px]">
              Start your reading journey now
            </p>
          </template>
        </UPageHeader>

        <Streak :week-dates="weekDates" />
      </div>

      <div class="grid grid-cols-12">
        <div class="col-span-12 md:col-span-6">
          <UTabs
            :items="tabs"
            :ui="{
              list: 'mb-2'
            }"
            class="max-w-[300px] mt-3 mb-2 mx-auto md:mx-0 md:mr-auto"
          >
            <template #books>
              <ReadingResources
                ref="books"
                @refresh="fetchRecommendation"
              />
            </template>
            <template #journal-paper>
              <ReadingResources
                ref="journals"
                journal
                @refresh="fetchRecommendation"
              />
            </template>
          </UTabs>
        </div>
        <div class="hidden md:flex sm:col-span-6 flex-row justify-center items-center">
          <nuxt-img
            src="/book-hero.png"
            class="max-h-[450px]"
          />
        </div>
      </div>

      <ReadingRecomendationList
        ref="recommendation"
        @refresh="fetchReadingResources"
      />

      <div
        v-if="reportPending"
        class="flex items-center justify-center py-12"
      >
        <UIcon
          name="i-heroicons-arrow-path"
          class="animate-spin text-4xl"
        />
      </div>

      <ReadingReportList
        v-else
        :reports="readingReports"
      />
    </div>
  </UContainer>
</template>
